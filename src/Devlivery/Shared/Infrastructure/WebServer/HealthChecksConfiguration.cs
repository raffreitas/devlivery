using Devlivery.Shared.Infrastructure.Identity.Context;
using Devlivery.Shared.Infrastructure.Persistence.Context;

namespace Devlivery.Shared.Infrastructure.WebServer;

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