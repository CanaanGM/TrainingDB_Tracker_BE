using System.Net;
using System.Net.Http.Json;

using API;
using DataLibrary.Context;
using DataLibrary.Models;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TrainingTests.APIControllersIntegrationTests;

/// <summary>
/// Drives the real HTTP pipeline through the auth endpoints. The existing AuthController tests
/// call the action methods directly and inspect the returned IActionResult object, so they never
/// execute the result — which is exactly how a broken <c>CreatedAtRoute</c> shipped: it only
/// throws during result execution. These tests exercise the endpoint over HTTP instead.
/// </summary>
public class AuthEndpointsIntegrationTests : IClassFixture<AuthEndpointsIntegrationTests.AuthApiFactory>
{
    private readonly AuthApiFactory _factory;

    public AuthEndpointsIntegrationTests(AuthApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_HappyPath_Returns2xx_WithToken()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.PostAsJsonAsync("/register", new
        {
            name = "pipeline user",
            email = "register-pipeline@example.com",
            password = "supersecret123"
        });

        // Regression guard: this endpoint previously returned 500 ("No route matches the supplied
        // values.") on success because CreatedAtRoute referenced a non-existent "Register" route.
        Assert.True(response.IsSuccessStatusCode,
            $"Expected 2xx, got {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");

        var payload = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.False(string.IsNullOrWhiteSpace(payload?.Token), "Successful registration must return a token.");
    }

    private sealed class AuthResponse
    {
        public string? Token { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
    }

    /// <summary>
    /// Boots the real API against a private in-memory SQLite DB, seeded with the "user" role that
    /// <c>AuthService.CreateUserAsync</c> assigns to new registrations.
    /// </summary>
    public class AuthApiFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection = new("DataSource=:memory:");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            _connection.Open();

            builder.ConfigureTestServices(services =>
            {
                foreach (var type in new[] { typeof(DbContextOptions<SqliteContext>), typeof(DbContextOptions), typeof(SqliteContext) })
                    foreach (var descriptor in services.Where(s => s.ServiceType == type).ToList())
                        services.Remove(descriptor);

                services.AddDbContext<SqliteContext>(options => options.UseSqlite(_connection));

                using var provider = services.BuildServiceProvider();
                using var scope = provider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<SqliteContext>();
                context.Database.EnsureCreated();
                context.Roles.Add(new Role { Name = "user" });
                context.SaveChanges();
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _connection.Dispose();
            base.Dispose(disposing);
        }
    }
}
