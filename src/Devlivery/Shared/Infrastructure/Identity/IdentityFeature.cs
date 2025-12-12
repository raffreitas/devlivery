using System.Text;
using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.Identity.Abstractions;
using Devlivery.Shared.Infrastructure.Identity.Authentication;
using Devlivery.Shared.Infrastructure.Identity.Context;
using Devlivery.Shared.Infrastructure.Identity.Tokens.Service;
using Devlivery.Shared.Infrastructure.Identity.Tokens.Settings;
using Devlivery.Shared.Infrastructure.Identity.Users.Models;
using Devlivery.Shared.Infrastructure.Identity.Users.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Devlivery.Shared.Infrastructure.Identity;

public static class IdentityFeature
{
    public static IServiceCollection AddIdentityFeature(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();

        services.AddAspNetIdentityConfiguration(configuration);
        services.AddTokensConfiguration(configuration);

        return services;
    }

    private static IServiceCollection AddTokensConfiguration(this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddScoped<ITokenService, JwtTokenService>();

        services.AddOptions<JwtTokenSettings>()
            .BindConfiguration(JwtTokenSettings.SectionName)
            .ValidateOnStart()
            .ValidateDataAnnotations();

        var jwtAuthOptions = configuration.GetOrThrow<JwtTokenSettings>(JwtTokenSettings.SectionName);

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtAuthOptions.Issuer,
                    ValidAudience = jwtAuthOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAuthOptions.SecretKey))
                };
            });

        return services;
    }

    private static IServiceCollection AddAspNetIdentityConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationIdentityDbContext>();

        services.AddScoped<IIdentityService, IdentityService>();

        var connectionString = configuration.GetConnectionStringOrThrow("DefaultConnection");
        services.AddDbContext<ApplicationIdentityDbContext>(options =>
        {
            options.UseNpgsql(connectionString, optionsBuilder => { optionsBuilder.EnableRetryOnFailure(); })
                .UseSnakeCaseNamingConvention();
        });

        return services;
    }
}