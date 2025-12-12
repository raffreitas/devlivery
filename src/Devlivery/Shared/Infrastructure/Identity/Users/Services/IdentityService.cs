using Devlivery.Features.Users.Domain;
using Devlivery.Shared.Infrastructure.Identity.Abstractions;
using Devlivery.Shared.Infrastructure.Identity.Users.Models;

using FluentResults;

using Microsoft.AspNetCore.Identity;

namespace Devlivery.Shared.Infrastructure.Identity.Users.Services;

internal sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : IIdentityService
{
    public async Task<Result> SignInAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return Result.Fail("Invalid credentials");

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, password, false);
        return !signInResult.Succeeded ? Result.Fail("Invalid credentials") : Result.Ok();
    }

    public async Task<IList<string>> GetRolesAsync(User user, CancellationToken cancellationToken = default)
    {
        var identityUser = await userManager.FindByEmailAsync(user.Email);
        if (identityUser is null) return [];
        return await userManager.GetRolesAsync(identityUser);
    }
}