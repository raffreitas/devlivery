using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;

using Devlivery.Infrastructure.Http.Models;

using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Devlivery.Infrastructure.Identity.LoginProtection;

public sealed class LoginRateLimitPolicy(
    IOptions<LoginProtectionOptions> options,
    ILogger<LoginRateLimitPolicy> logger) : IRateLimiterPolicy<string>
{
    public const string Name = "login";

    public RateLimitPartition<string> GetPartition(HttpContext httpContext) =>
        RateLimitPartition.GetFixedWindowLimiter(GetClientKey(httpContext, options.Value.RailwayIngress),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = options.Value.PermitLimit,
                Window = TimeSpan.FromSeconds(options.Value.WindowSeconds),
                QueueLimit = 0,
                AutoReplenishment = true
            });

    private static string GetClientKey(HttpContext context, bool railwayIngress)
    {
        var address = context.Connection.RemoteIpAddress;
        if (railwayIngress)
        {
            var values = context.Request.Headers["X-Real-IP"];
            address = values.Count == 1 && !values[0]!.Contains(',') && IPAddress.TryParse(values[0], out var parsed)
                ? parsed
                : null;
        }

        if (address?.IsIPv4MappedToIPv6 == true) address = address.MapToIPv4();
        return address?.ToString() ?? "unknown";
    }

    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected => async (context, ct) =>
    {
        var seconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
            ? Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds))
            : options.Value.WindowSeconds;

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.Headers.RetryAfter = seconds.ToString(CultureInfo.InvariantCulture);

        logger.LogWarning("Login rate limit exceeded. Policy: {Policy}", Name);

        await context.HttpContext.Response.WriteAsJsonAsync(
            new ApiProblemDetails
            {
                Title = "Muitas tentativas",
                Status = StatusCodes.Status429TooManyRequests,
                Detail = "Muitas tentativas. Aguarde antes de tentar novamente."
            }, cancellationToken: ct);
    };
}