using Microsoft.AspNetCore.Authorization;

namespace Devlivery.WebApi.Shared.Authorization;

public static class AuthorizationFeature
{
    public static IServiceCollection AddAuthorizationFeature(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());

        return services;
    }

    public static IApplicationBuilder UseAuthorizationFeature(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }
}