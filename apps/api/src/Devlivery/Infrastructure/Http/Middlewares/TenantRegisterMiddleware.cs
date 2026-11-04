using System.Diagnostics;

using Devlivery.Infrastructure.Tenancy;

using Microsoft.AspNetCore.Authorization;

namespace Devlivery.Infrastructure.Http.Middlewares;

public class TenantRegisterMiddleware(
    ITenantLocator tenantLocator,
    ITenantAccessor tenantAccessor
) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
        {
            await next(context);
            return;
        }

        var tenant = await tenantLocator.GetAsync(context.RequestAborted);
        tenantAccessor.Register(tenant);
        Activity.Current?.SetTag("tenant.id", tenant.Id);

        await next(context);
    }
}
