using System.Security.Claims;
using System.Text;
using Devlivery.WebApi.Shared.Identity.Abstractions;
using Devlivery.WebApi.Shared.Identity.Tokens.Settings;
using Devlivery.WebApi.Shared.Tenancy;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Devlivery.WebApi.Shared.Identity.Tokens.Service;

internal sealed class JwtTokenService(IOptions<JwtTokenSettings> options) : ITokenService
{
    private readonly JwtTokenSettings _settings = options.Value;

    public Task<string> GenerateTokenAsync(TokenRequest tokenRequest, CancellationToken cancellationToken = default)
    {
        var signinKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(signinKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, tokenRequest.SubjectId),
            new(TenantConstants.TenantIdClaimType, tokenRequest.TenantId)
        ];

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_settings.ExpirationInMinutes),
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JsonWebTokenHandler();
        var accessToken = tokenHandler.CreateToken(tokenDescriptor);

        return Task.FromResult(accessToken);
    }
}