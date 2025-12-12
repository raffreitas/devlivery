using Devlivery.Shared.Database.Abstractions;
using Devlivery.Shared.Database.Context;
using Devlivery.Shared.Database.Factory;
using Devlivery.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Devlivery.Shared.Database;

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