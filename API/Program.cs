using API.Security;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using System.Text;

namespace API;

using API.Common.Errors;
using API.Common.Filters;
using API.Common.Middleware;
using API.Common.Providers;

using DataLibrary;

using Microsoft.AspNetCore.Mvc.Infrastructure;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddDataLibrary();


        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["TokenKey"]));

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IUserAccessor, UserAccessor>();
        builder.Services.AddScoped<AuthenticatedUserFilter>();


        builder.Services.AddSingleton<ProblemDetailsFactory, TrainingProblemDetailsFactory>();
        builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseMiddleware<ExceptionMiddleware>();
        app.UseMiddleware<RequestLoggerMiddleWare>();

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
