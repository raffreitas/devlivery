namespace Devlivery.Shared.Infrastructure.Networking;

public static class NetworkingFeature
{
    public static IServiceCollection AddNetworkingFeature(this IServiceCollection services)
    {
        services.AddServiceDiscovery();

        services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        return services;
    }
}