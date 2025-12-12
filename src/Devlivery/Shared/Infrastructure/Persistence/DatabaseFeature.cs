using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.Persistence.Abstractions;
using Devlivery.Shared.Infrastructure.Persistence.Context;
using Devlivery.Shared.Infrastructure.Persistence.Factory;
using Devlivery.Shared.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Devlivery.Shared.Infrastructure.Persistence;

public static class DatabaseFeature
{
    public static IServiceCollection AddDatabaseFeature(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionStringOrThrow("DefaultConnection");
        
        // Register the interceptor
        services.AddScoped<DispatchDomainEventsInterceptor>();
        
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var interceptor = serviceProvider.GetRequiredService<DispatchDomainEventsInterceptor>();
            
            options.UseNpgsql(connectionString, optionsBuilder => { optionsBuilder.EnableRetryOnFailure(); })
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(interceptor);
        });

        services.AddSingleton(new NpgsqlDataSourceBuilder(connectionString).Build());
        services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

        return services;
    }
}