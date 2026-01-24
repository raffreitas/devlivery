using Microsoft.AspNetCore.Authorization;

namespace Devlivery.Shared.Infrastructure.Authorization;

public static class AuthorizationFeature
{
    public static IServiceCollection AddAuthorizationFeature(this IServiceCollection services)
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