using FluentResults;

namespace Devlivery.Infrastructure.Identity.Abstractions;

public interface IIdentityService
{
    Task<Result> SignInAsync(string email, string password, CancellationToken cancellationToken = default);
}