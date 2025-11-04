using Devlivery.WebApi.Features.Users.Domain;
using FluentResults;

namespace Devlivery.WebApi.Shared.Abstractions;

public interface ITokenService
{
    Task<Result<string>> GenerateTokenAsync(User user, CancellationToken cancellationToken = default);
}