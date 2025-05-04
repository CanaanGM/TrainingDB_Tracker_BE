using System.Globalization;
using API.Security;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using System.Text;
using System.Threading.RateLimiting;
using API.Common.Validators;
using DataLibrary.Models;
using FluentValidation;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
using SharedLibrary.Dtos;

namespace API;

using API.Common.Errors;
using API.Common.Filters;
using API.Common.Middleware;
using API.Common.Providers;
using API.Resources;
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
        builder.Services.AddSwaggerGen(options =>
        {
            options.EnableAnnotations();
            options.OperationFilter<AcceptLanguageHeaderOperationFilter>();
        } );

        builder.Services.AddDataLibrary();

        //localization
        var supportedCultures = new CultureInfo[] {new ("en-US"), new("ja"), new ("ar") };
        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
            options.DefaultRequestCulture = new RequestCulture(supportedCultures.First());  
        });
        
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<IUserAccessor, UserAccessor>();
        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.JwtSettingsSection));
        
        var keyFromConfig = builder.Configuration["JwtSettings:Secret"]
                            ?? throw new KeyNotFoundException("define a token key in app settings please!"); 
        
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true, 
                    IssuerSigningKey = new SymmetricSecurityKey(
	                   Encoding.UTF8.GetBytes(keyFromConfig)), 
                    ValidateIssuer = true, // the domain of which the token was issued
                    ValidateAudience = true, // the target domains for the token
                    ValidAudience = builder.Configuration["JwtSettings:Audience"],
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                    ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha512 },
                    ValidateLifetime = true
                };
            });
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IUserAccessor, UserAccessor>();
        builder.Services.AddScoped<AuthenticatedUserFilter>();
        builder.Services.AddRateLimiter(rateLimiterOptions =>
        {
            rateLimiterOptions.AddSlidingWindowLimiter("login", options =>
            {
                options.PermitLimit = 5; // 5 login attempts
                options.Window = TimeSpan.FromMinutes(15); // per 15 minutes
                options.SegmentsPerWindow = 3; // divides window into 3 segments (5 min each)
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 0; // no queuing for login attempts
            });

            rateLimiterOptions.AddPolicy("strict-login", context =>
            {
                context.Request.EnableBuffering();
                //get the connecting ip
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";
                // combine IP 
                string partitionKey = $"{ipAddress}";
                return RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: partitionKey
                    , factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 5
                        , Window = TimeSpan.FromMinutes(15)
                        , SegmentsPerWindow = 3
                    });
            });
            
            rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            rateLimiterOptions.OnRejected = async (context, rejectionReason) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsync("Calm thy tits! too many requests!", rejectionReason);
                
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning(
                    $"Rate limit exceeded for {context.HttpContext.Connection.RemoteIpAddress}");
            };
        });

        builder.Services.AddSingleton<ProblemDetailsFactory, TrainingProblemDetailsFactory>();
        builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(5134);
        });
        
        WebApplication app = builder.Build();
        
        app.MapGet("/culture", () => Thread.CurrentThread.CurrentCulture.DisplayName);
        app.MapGet("/helloworld", () => Messages.HelloWorld);
        app.MapGet("/hello", (string name) =>
        {
            var message = string.Format(Messages.GreetingMessage, name);
            return message;
        });
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
        app.UseRateLimiter();
        app.UseRequestLocalization();
        app.MapControllers();

        app.Run();
    }
}