using System.Text;
using Devlivery.WebApi.Shared.Extensions;
using Devlivery.WebApi.Shared.Identity.Abstractions;
using Devlivery.WebApi.Shared.Identity.Authentication;
using Devlivery.WebApi.Shared.Identity.Context;
using Devlivery.WebApi.Shared.Identity.Tokens.Service;
using Devlivery.WebApi.Shared.Identity.Tokens.Settings;
using Devlivery.WebApi.Shared.Identity.Users.Models;
using Devlivery.WebApi.Shared.Identity.Users.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Devlivery.WebApi.Shared.Identity;

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