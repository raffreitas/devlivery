using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Infrastructure.Identity.Abstractions;
using Devlivery.Shared.Infrastructure.Persistence.Context;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.Auth.Commands.Login;

public sealed class LoginHandler(
    ILogger<LoginHandler> logger,
    ApplicationDbContext dbContext,
    IIdentityService identityService,
    ITokenService tokenService) : ICommandHandler<LoginCommand, Result<LoginResponse>>
{
    public async ValueTask<Result<LoginResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == command.Email, cancellationToken);

        if (user is null)
        {
            logger.LogInformation("Failed login attempt.");
            return Result.Fail<LoginResponse>(new UnauthorizedError());
        }

        var signInResult = await identityService.SignInAsync(user.Email, command.Password, cancellationToken);
        if (signInResult.IsFailed)
        {
            logger.LogInformation("Failed login attempt.");
            return Result.Fail<LoginResponse>(new UnauthorizedError());
        }

        var tokenRequest = new TokenRequest(
            user.Id.ToString(),
            user.EstablishmentId.ToString()
        );

        var token = await tokenService.GenerateTokenAsync(tokenRequest, cancellationToken);

        return new LoginResponse(user.Id, user.Name, token);
    }
}