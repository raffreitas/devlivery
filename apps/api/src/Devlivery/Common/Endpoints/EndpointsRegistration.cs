namespace Devlivery.Common.Endpoints;

public static class EndpointsRegistration
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblyOf<IEndpoint>()
            .AddClasses(classes => classes.AssignableTo<IEndpoint>(), publicOnly: false)
                .As<IEndpoint>()
                .WithSingletonLifetime());

        return services;
    }

    public static IEndpointRouteBuilder MapEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        foreach (var endpoint in app.Services.GetRequiredService<IEnumerable<IEndpoint>>())
        {
            endpoint.MapEndpoint(api);
        }

        return app;
    }
}