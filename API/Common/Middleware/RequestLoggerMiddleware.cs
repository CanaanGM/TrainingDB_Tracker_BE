using API.Common.Providers;

namespace API.Common.Middleware;

public class RequestLoggerMiddleWare
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggerMiddleWare> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RequestLoggerMiddleWare(
        RequestDelegate next,
        ILogger<RequestLoggerMiddleWare> logger,
        IDateTimeProvider dateTimeProvider)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            ExtractLogs(context);
            await _next(context);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private void ExtractLogs(HttpContext context)
    {
        var userAgent = context.Request.Headers["User-Agent"];
        var host = context.Request.Headers.Host;
        var auth = context.Request.Headers["Authorization"];
        var contentType = context.Request.Headers.ContentType;
        var acceptLanguageHeader = context.Request.Headers["Accept-Language"];
        var remoteIpAddress = context.Connection.RemoteIpAddress;

        _logger.LogInformation($"A request was made by the agent: {userAgent}.\nAt:\n\t{_dateTimeProvider.UtcNow} UTC.\n\t{_dateTimeProvider.LocalNow} Local.\nwith the auth header\n{auth}\nfrom: IP:{remoteIpAddress}.\nhost: '{host}\ncontent type: {contentType}\n'.");
    }
}