using Devlivery.Common.Events;
using Devlivery.Common.Extensions;
using Devlivery.Infrastructure.Identity.Context;
using Devlivery.Infrastructure.Identity.Users.Models;
using Devlivery.Infrastructure.Persistence.Abstractions;
using Devlivery.Infrastructure.Persistence.Context;
using Devlivery.Infrastructure.Persistence.Factory;
using Devlivery.Infrastructure.Persistence.Seeder;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Npgsql;

namespace Devlivery.Infrastructure.Persistence;

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

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static WebApplication UseDatabaseFeature(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()) return app;

        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var idContext = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        dbContext.Database.Migrate();
        idContext.Database.Migrate();
        DatabaseSeeder.SeedAsync(dbContext, userManager).GetAwaiter().GetResult();

        return app;
    }
}