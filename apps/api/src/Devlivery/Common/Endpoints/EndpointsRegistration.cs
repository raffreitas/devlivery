using Asp.Versioning;

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
        var versions = app.Services.GetRequiredService<IEnumerable<IEndpoint>>()
            .GroupBy(endpoint => endpoint.Version);
        foreach (var version in versions)
        {
            var api = app.NewVersionedApi()
                .MapGroup("/api/v{version:apiVersion}")
                .HasApiVersion(version.Key);

            foreach (var endpoint in version)
            {
                endpoint.MapEndpoint(api);
            }
        }

        return app;
    }
}
