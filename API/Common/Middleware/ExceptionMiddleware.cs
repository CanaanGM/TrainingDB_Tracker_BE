using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

namespace API.Common.Middleware;
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (Exception ex) { await HandleExceptionAsync(context, ex); }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        _logger.LogError(ex, "An error occurred.");

        var problemDetails = new ProblemDetails
        {
            Title = "A problem occurred while processing your request, please try again later.",
            Detail = ex.Message,
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://example.com/errors/internal-server-error", // Your custom error type URL
        };

        // Optionally, you can add custom headers to the response, such as 'WWW-Authenticate'
        //context.Response.Headers.Add("WWW-Authenticate", new StringValues("Bearer realm=\"example\""));

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(JsonConvert.SerializeObject(problemDetails));
    }
}
