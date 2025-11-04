using Devlivery.WebApi.Shared.Infrastructure.Database.Context;
using Devlivery.WebApi.Shared.Infrastructure.Identity.Models;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.Auth.Commands.Login;

public sealed class LoginHandler(
    ApplicationDbContext dbContext,
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager)
{
    public async Task<Result<LoginResponse>> HandleAsync(
        LoginCommand request,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken: cancellationToken);

        if (user is null)
            return Result.Fail("Invalid credentials");

        var identityUser = await userManager.FindByEmailAsync(user.Email);
        if (identityUser is null)
            return Result.Fail("Invalid credentials");

        var signInResult = await signInManager.CheckPasswordSignInAsync(identityUser, request.Password, false);
        if (!signInResult.Succeeded)
            return Result.Fail("Invalid credentials");

        var token = $"mock-token-{Guid.NewGuid()}";
        return new LoginResponse(user.Id, user.Name, token);
    }
}