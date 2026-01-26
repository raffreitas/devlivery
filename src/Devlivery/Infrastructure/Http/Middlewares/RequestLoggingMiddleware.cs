using System.Diagnostics;

namespace Devlivery.Infrastructure.Http.Middlewares;

internal sealed class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        var logState = new Dictionary<string, object>
        {
            ["RequestMethod"] = context.Request.Method,
            ["RequestPath"] = context.Request.Path,
        };

        using (logger.BeginScope(logState))
        {
            await next(context);
        }

        sw.Stop();

        logger.LogInformation("HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds}ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            sw.ElapsedMilliseconds
        );
    }
}