namespace Devlivery.WebApi.Shared.Presentation;

public static class CorsConfiguration
{
    private const string DefaultPolicyName = "DefaultCorsPolicy";

    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(DefaultPolicyName, policy =>
            {
                var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS") 
                                   ?? "http://localhost:5173";
                
                // Dividir origens permitidas (separadas por vírgula)
                var origins = allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                           .Select(o => o.Trim())
                                           .ToArray();

                policy.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .SetIsOriginAllowedToAllowWildcardSubdomains(); // Para *.railway.app em review apps
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