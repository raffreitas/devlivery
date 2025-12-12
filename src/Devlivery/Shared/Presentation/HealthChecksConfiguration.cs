using Devlivery.Shared.Database.Context;
using Devlivery.Shared.Identity.Context;

namespace Devlivery.Shared.Presentation;

public static class HealthChecksConfiguration
{
    public static IServiceCollection AddHealthChecksConfiguration(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>(name: "app-db")
            .AddDbContextCheck<ApplicationIdentityDbContext>(name: "identity-db");
        return services;
    }
}