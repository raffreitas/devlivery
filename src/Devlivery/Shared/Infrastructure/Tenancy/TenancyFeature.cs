using Devlivery.Shared.Infrastructure.Tenancy.Behaviors;
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
        
        // Register pipeline behavior for automatic tenant context in domain events
        services.AddScoped(typeof(Mediator.IPipelineBehavior<,>), typeof(DomainEventTenantBehavior<>));
        
        return services;
    }

    public static IApplicationBuilder UseTenancyFeature(this IApplicationBuilder app)
    {
        app.UseMiddleware<TenantRegisterMiddleware>();
        return app;
    }
}