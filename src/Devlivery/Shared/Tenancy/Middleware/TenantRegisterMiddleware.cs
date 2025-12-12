using System.Diagnostics;

namespace Devlivery.Shared.Tenancy.Middleware;

public class TenantRegisterMiddleware(
    ITenantLocator tenantLocator,
    ITenantAccessor tenantAccessor
) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (
            context.Request.Path.Value == null ||
            context.Request.Path.Value.Contains("/scalar") ||
            context.Request.Path.Value.Contains("/openapi") ||
            context.Request.Path.Value.Contains("/health") ||
            context.Request.Path.Value.Contains("/login"))
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