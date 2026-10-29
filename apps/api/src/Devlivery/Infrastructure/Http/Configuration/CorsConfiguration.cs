using Devlivery.Common.Extensions;

namespace Devlivery.Infrastructure.Http.Configuration;

public static class CorsConfiguration
{
    private const string DefaultPolicyName = "DefaultCorsPolicy";
    public const string AllowedOriginsKey = "ALLOWED_ORIGINS";

    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetValueOrThrow<string>(AllowedOriginsKey)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (origins.Length == 0)
            throw new InvalidOperationException($"The value {AllowedOriginsKey} must list at least one origin");

        services.AddCors(options =>
        {
            options.AddPolicy(DefaultPolicyName, policy =>
            {
                policy.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    public static IApplicationBuilder UseCorsConfiguration(this IApplicationBuilder app)
    {
        app.UseCors(DefaultPolicyName);
        return app;
    }
}