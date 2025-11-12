using Devlivery.WebApi.Shared.Tenancy.Middleware;

namespace Devlivery.WebApi.Shared.Tenancy;

public static class TenancyFeature
{
    public static IServiceCollection AddTenancyFeature(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantAccessor, TenantAccessor>();
        services.AddScoped<ITenantLocator, TenantLocator>();
        return services;
    }

    public static IApplicationBuilder UseTenancyFeature(this IApplicationBuilder app)
    {
        app.UseMiddleware<TenantRegisterMiddleware>();
        return app;
    }
}