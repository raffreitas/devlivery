using System.Text;
using Devlivery.WebApi.Shared.Extensions;
using Devlivery.WebApi.Shared.Infrastructure.Tokens.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Devlivery.WebApi.Shared.Infrastructure.Tokens;

public static class TokenFeature
{
    public static IServiceCollection AddTokensFeature(this IServiceCollection services, IConfiguration configuration)
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

        return services;
    }
}