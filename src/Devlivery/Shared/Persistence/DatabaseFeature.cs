using Devlivery.Shared.Extensions;
using Devlivery.Shared.Persistence.Abstractions;
using Devlivery.Shared.Persistence.Context;
using Devlivery.Shared.Persistence.Factory;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Devlivery.Shared.Persistence;

public static class DatabaseFeature
{
    public static IServiceCollection AddDatabaseFeature(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionStringOrThrow("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, optionsBuilder => { optionsBuilder.EnableRetryOnFailure(); })
                .UseSnakeCaseNamingConvention();
        });

        services.AddSingleton(new NpgsqlDataSourceBuilder(connectionString).Build());
        services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

        return services;
    }
}