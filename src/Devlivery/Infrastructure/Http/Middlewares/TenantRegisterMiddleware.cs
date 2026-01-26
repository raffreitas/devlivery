using System.Diagnostics;

using Devlivery.Infrastructure.Tenancy;

namespace Devlivery.Infrastructure.Http.Middlewares;

public class TenantRegisterMiddleware(
    ITenantLocator tenantLocator,
    ITenantAccessor tenantAccessor
) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        string[] publicEndpoints =
        [
            "/scalar",
            "/openapi",
            "/health",
            "/alive",
            "/api/auth/login"
        ];
        if (publicEndpoints.Any(endpoint => context.Request.Path.StartsWithSegments(endpoint)))
        {
            await next(context);
            return;
        }

        try
        {
            var tenant = await tenantLocator.GetAsync(context.RequestAborted);
            tenantAccessor.Register(tenant);

            Activity.Current?.SetTag("tenant.id", tenant.Id);
            await next(context);
        }
        catch (UnauthorizedAccessException)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.CompleteAsync();
        }
    }
}