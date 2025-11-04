using Devlivery.WebApi.Shared.Extensions;
using Devlivery.WebApi.Shared.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Shared.Infrastructure.Database;

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

        return services;
    }
}