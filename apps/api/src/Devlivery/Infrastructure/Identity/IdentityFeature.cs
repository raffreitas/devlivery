using Devlivery.Infrastructure.Identity.LoginProtection;

using Microsoft.Extensions.Options;

using System.Text;

using Devlivery.Common.Extensions;
using Devlivery.Infrastructure.Identity.Abstractions;
using Devlivery.Infrastructure.Identity.Authentication;
using Devlivery.Infrastructure.Identity.Context;
using Devlivery.Infrastructure.Identity.Tokens.Service;
using Devlivery.Infrastructure.Identity.Tokens.Settings;
using Devlivery.Infrastructure.Identity.Users.Models;
using Devlivery.Infrastructure.Identity.Users.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Devlivery.Infrastructure.Identity;

public static class IdentityFeature
{
    public static IServiceCollection AddIdentityFeature(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
        services.AddOptions<LoginProtectionOptions>().BindConfiguration(LoginProtectionOptions.SectionName)
            .ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<IdentityOptions>().Configure<IOptions<LoginProtectionOptions>>((identity, protection) =>
        {
            identity.Lockout.AllowedForNewUsers = true;
            identity.Lockout.MaxFailedAccessAttempts = protection.Value.MaxFailedAccessAttempts;
            identity.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(protection.Value.LockoutMinutes);
        });
        services.AddRateLimiter(options => options.AddPolicy<string, LoginRateLimitPolicy>(LoginRateLimitPolicy.Name));

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
                options.MapInboundClaims = false;
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