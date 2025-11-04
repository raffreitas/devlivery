using System.Text;
using Devlivery.WebApi.Features.Auth.Abstractions;
using Devlivery.WebApi.Features.Auth.Commands.Login;
using Devlivery.WebApi.Features.Auth.Infrastructure.Tokens.Service;
using Devlivery.WebApi.Features.Auth.Infrastructure.Tokens.Settings;
using Devlivery.WebApi.Shared.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Devlivery.WebApi.Features.Auth;

public static class AuthFeature
{
    public static IServiceCollection AddAuthFeature(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<LoginHandler>();

        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddTokensConfiguration(configuration);
        return services;
    }

    private static void AddTokensConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
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

        services.AddAuthorization();
    }

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        LoginEndpoint.MapEndpoint(group);

        return app;
    }
}