using Devlivery.Common.Errors;
using Devlivery.Features.Users.Domain;
using Devlivery.Infrastructure.Identity.Abstractions;
using Devlivery.Infrastructure.Identity.Users.Models;

using FluentResults;

using Microsoft.AspNetCore.Identity;

namespace Devlivery.Infrastructure.Identity.Users.Services;

internal sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : IIdentityService
{
    public async Task<Result> SignInAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return Result.Fail(new UnauthorizedError());

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, password, false);
        return !signInResult.Succeeded ? Result.Fail(new UnauthorizedError()) : Result.Ok();
    }

    public async Task<IList<string>> GetRolesAsync(User user, CancellationToken cancellationToken = default)
    {
        var identityUser = await userManager.FindByEmailAsync(user.Email);
        if (identityUser is null) return new List<string>();
        return await userManager.GetRolesAsync(identityUser);
    }
}