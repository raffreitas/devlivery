namespace Devlivery.Shared.Extensions;

public static class ConfigurationExtensions
{
    public static string GetConnectionStringOrThrow(this IConfiguration configuration, string name) =>
        configuration.GetConnectionString(name) ?? throw new InvalidOperationException($"The connection string {name} was not found");

    public static T GetValueOrThrow<T>(this IConfiguration configuration, string name) =>
        configuration.GetValue<T?>(name) ?? throw new InvalidOperationException($"The value {name} was not found");

    public static T GetOrThrow<T>(this IConfiguration configuration, string name) =>
    configuration.GetSection(name).Get<T?>() ?? throw new InvalidOperationException($"The section {name} was not found");
}