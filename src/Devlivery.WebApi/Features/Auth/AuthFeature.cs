using Devlivery.WebApi.Features.Auth.Commands.Login;

namespace Devlivery.WebApi.Features.Auth;

public static class AuthFeature
{
    public static IServiceCollection AddAuthFeature(this IServiceCollection services, IConfiguration configuration)
    {
        // Handlers
        services.AddScoped<LoginHandler>();

        return services;
    }

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        LoginEndpoint.MapEndpoint(group);

        return app;
    }
}