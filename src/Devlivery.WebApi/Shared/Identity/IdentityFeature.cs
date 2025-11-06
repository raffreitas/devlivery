using Devlivery.WebApi.Shared.Extensions;
using Devlivery.WebApi.Shared.Identity.Context;
using Devlivery.WebApi.Shared.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Shared.Identity;

public static class IdentityFeature
{
    public static IServiceCollection AddIdentityFeature(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationIdentityDbContext>();

        var connectionString = configuration.GetConnectionStringOrThrow("DefaultConnection");
        services.AddDbContext<ApplicationIdentityDbContext>(options =>
        {
            options.UseNpgsql(connectionString, optionsBuilder => { optionsBuilder.EnableRetryOnFailure(); })
                .UseSnakeCaseNamingConvention();
        });

        return services;
    }
}