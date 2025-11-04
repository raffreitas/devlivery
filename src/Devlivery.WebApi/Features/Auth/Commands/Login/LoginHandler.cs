using Devlivery.WebApi.Features.Auth.Abstractions;
using Devlivery.WebApi.Shared.Infrastructure.Database.Context;
using Devlivery.WebApi.Shared.Infrastructure.Identity.Models;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.Auth.Commands.Login;

public sealed class LoginHandler(
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
            return Result.Fail("Credenciais inválidas");

        var identityUser = await userManager.FindByEmailAsync(user.Email);
        if (identityUser is null)
            return Result.Fail("Credenciais inválidas");

        var signInResult = await signInManager.CheckPasswordSignInAsync(identityUser, request.Password, false);
        if (!signInResult.Succeeded)
            return Result.Fail("Credenciais inválidas");

        var tokenRequest = new TokenRequest(user.Id.ToString(), user.Email);
        var token = await tokenService.GenerateTokenAsync(tokenRequest, cancellationToken);

        return new LoginResponse(user.Id, user.Name, token);
    }
}