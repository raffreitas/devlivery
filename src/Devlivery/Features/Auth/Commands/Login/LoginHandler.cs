using Devlivery.Shared.Infrastructure.Identity.Abstractions;
using Devlivery.Shared.Infrastructure.Persistence.Context;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.Auth.Commands.Login;

public sealed class LoginHandler(
    ILogger<LoginHandler> logger,
    ApplicationDbContext dbContext,
    IIdentityService identityService,
    ITokenService tokenService)
{
    public async Task<Result<LoginResponse>> HandleAsync(LoginCommand request,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

        if (user is null)
        {
            logger.LogInformation("Failed login attempt.");
            return Result.Fail("Credenciais inválidas");
        }

        var signInResult = await identityService.SignInAsync(user.Email, request.Password, cancellationToken);
        if (signInResult.IsFailed)
        {
            logger.LogInformation("Failed login attempt.");
            return Result.Fail("Credenciais inválidas");
        }

        var tokenRequest = new TokenRequest(
            user.Id.ToString(),
            user.EstablishmentId.ToString()
        );

        var token = await tokenService.GenerateTokenAsync(tokenRequest, cancellationToken);

        return new LoginResponse(user.Id, user.Name, token);
    }
}