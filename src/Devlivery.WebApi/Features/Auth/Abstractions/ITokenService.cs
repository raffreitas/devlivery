namespace Devlivery.WebApi.Features.Auth.Abstractions;

public interface ITokenService
{
    Task<string> GenerateTokenAsync(TokenRequest tokenRequest, CancellationToken cancellationToken = default);
}

public sealed record TokenRequest(string SubjectId, string TenantId, string Email);