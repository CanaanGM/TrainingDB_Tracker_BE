using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;

using API;
using API.Security;

using DataLibrary.Context;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SharedLibrary.Dtos;

namespace TrainingTests.APIControllersIntegrationTests;

/// <summary>
/// Exercises the *real* HTTP pipeline (routing + JWT authentication + the
/// <see cref="API.Common.Filters.AuthenticatedUserFilter"/>) end to end against a protected
/// endpoint. The existing controller tests mock <c>IUserAccessor</c>, so they never touch token
/// validation; these lock in the observable auth contract instead.
/// </summary>
public class AuthPipelineIntegrationTests : IClassFixture<AuthPipelineIntegrationTests.AuthPipelineFactory>
{
    private readonly AuthPipelineFactory _factory;

    public AuthPipelineIntegrationTests(AuthPipelineFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/measurements");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithValidToken_Returns200()
    {
        var token = CreateTokenForUser(userId: 1);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/measurements");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithTokenSignedByWrongKey_Returns401()
    {
        // A token with a valid shape but the wrong signature must be rejected — proves the
        // pipeline actually validates the signature rather than trusting any bearer token.
        var forged = ForgeTokenWithWrongKey(userId: 1);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", forged);

        var response = await client.GetAsync("/measurements");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public void IssuedToken_HasNonTrivialLifetime()
    {
        // Regression guard for the JwtSettings binding: the appsettings key must match the
        // JwtSettings.ExpiryMinutes property, otherwise it binds to 0 and every token expires
        // immediately (surviving only on the JWT handler's default ~5 min clock skew).
        var token = CreateTokenForUser(userId: 1);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        var lifetime = jwt.ValidTo - jwt.ValidFrom;

        Assert.True(lifetime > TimeSpan.FromMinutes(30),
            $"Token lifetime was {lifetime}; JwtSettings:ExpiryMinutes is likely unbound (0).");
    }

    private string CreateTokenForUser(int userId)
    {
        // Use the app's own TokenService so the token is signed/scoped exactly how the
        // pipeline expects to validate it (same secret, issuer, audience, algorithm).
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        return tokenService.CreateToken(new InternalUserAuthDto
        {
            Id = userId,
            Username = "pipeline-tester",
            Email = "tester@example.com",
            Roles = new List<string>()
        });
    }

    private static string ForgeTokenWithWrongKey(int userId)
    {
        var wrongKey = new string('z', 80); // valid HMAC-SHA512 key length, wrong secret
        var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(
            new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(wrongKey)),
            Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha512Signature);
        var descriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(
                new[] { new System.Security.Claims.Claim(
                    System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString()) }),
            Audience = "http://localhost",
            Issuer = "http://localhost",
            SigningCredentials = creds,
            Expires = DateTime.UtcNow.AddMinutes(30),
        };
        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(descriptor));
    }

    /// <summary>
    /// Boots the real API but swaps the hard-coded SQLite file for a private in-memory
    /// database so the pipeline runs without touching the developer's machine.
    /// </summary>
    public class AuthPipelineFactory : WebApplicationFactory<Program>
    {
        // Held open for the lifetime of the factory: an in-memory SQLite DB only exists
        // while at least one connection to it is open.
        private readonly SqliteConnection _connection = new("DataSource=:memory:");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            _connection.Open();

            builder.ConfigureTestServices(services =>
            {
                RemoveDescriptor(services, typeof(DbContextOptions<SqliteContext>));
                RemoveDescriptor(services, typeof(DbContextOptions));
                RemoveDescriptor(services, typeof(SqliteContext));

                services.AddDbContext<SqliteContext>(options => options.UseSqlite(_connection));

                using var provider = services.BuildServiceProvider();
                using var scope = provider.CreateScope();
                scope.ServiceProvider.GetRequiredService<SqliteContext>().Database.EnsureCreated();
            });
        }

        private static void RemoveDescriptor(IServiceCollection services, Type serviceType)
        {
            foreach (var descriptor in services.Where(d => d.ServiceType == serviceType).ToList())
                services.Remove(descriptor);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _connection.Dispose();
            base.Dispose(disposing);
        }
    }
}
