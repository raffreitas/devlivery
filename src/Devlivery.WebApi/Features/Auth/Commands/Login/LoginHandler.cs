using Devlivery.WebApi.Features.Auth.Abstractions;
using Devlivery.WebApi.Shared.Database.Context;
using Devlivery.WebApi.Shared.Identity.Models;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.Auth.Commands.Login;

public sealed class LoginHandler(
    ILogger<LoginHandler> logger,
    ApplicationDbContext dbContext,
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService)
{
    public async Task<Result<LoginResponse>> HandleAsync(
        LoginCommand request,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken: cancellationToken);

        if (user is null)
        {
            logger.LogInformation("Failed login attempt for email: {Email}", request.Email);
            return Result.Fail("Credenciais inválidas");
        }

        var identityUser = await userManager.FindByEmailAsync(user.Email);
        if (identityUser is null)
        {
            logger.LogInformation("Failed login attempt for email: {Email}", request.Email);
            return Result.Fail("Credenciais inválidas");
        }

        var signInResult = await signInManager.CheckPasswordSignInAsync(identityUser, request.Password, false);
        if (!signInResult.Succeeded)
        {
            logger.LogInformation("Failed login attempt for email: {Email}", request.Email);
            return Result.Fail("Credenciais inválidas");
        }

        var tokenRequest = new TokenRequest(user.Id.ToString(), user.Email);
        var token = await tokenService.GenerateTokenAsync(tokenRequest, cancellationToken);

        return new LoginResponse(user.Id, user.Name, token);
    }
}