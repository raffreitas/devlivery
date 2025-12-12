using Devlivery.Shared.Infrastructure.Tenancy.Middleware;

namespace Devlivery.Shared.Infrastructure.Tenancy;

public static class TenancyFeature
{
    public static IServiceCollection AddTenancyFeature(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantAccessor, TenantAccessor>();
        services.AddScoped<ITenantLocator, TenantLocator>();
        services.AddScoped<TenantRegisterMiddleware>();
        return services;
    }

    public static IApplicationBuilder UseTenancyFeature(this IApplicationBuilder app)
    {
        app.UseMiddleware<TenantRegisterMiddleware>();
        return app;
    }
}