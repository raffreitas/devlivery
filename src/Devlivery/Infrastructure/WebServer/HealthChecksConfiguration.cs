using Devlivery.Infrastructure.Identity.Context;
using Devlivery.Infrastructure.Persistence.Context;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Devlivery.Infrastructure.WebServer;

public static class HealthChecksConfiguration
{
    public static IServiceCollection AddHealthChecksConfiguration(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>(name: "app-db")
            .AddDbContextCheck<ApplicationIdentityDbContext>(name: "identity-db")
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return services;
    }

    public static WebApplication MapHealthCheckEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health").AllowAnonymous();
        app.MapHealthChecks("/alive", new HealthCheckOptions { Predicate = r => r.Tags.Contains("live") })
            .AllowAnonymous();

        return app;
    }
}