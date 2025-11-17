using FluentResults;

namespace Devlivery.WebApi.Shared.Identity.Abstractions;

public interface IIdentityService
{
    Task<Result> SignInAsync(string email, string password, CancellationToken cancellationToken = default);
}