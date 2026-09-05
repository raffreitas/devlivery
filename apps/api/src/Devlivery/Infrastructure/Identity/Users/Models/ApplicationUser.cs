using Microsoft.AspNetCore.Identity;

namespace Devlivery.Infrastructure.Identity.Users.Models;

public sealed class ApplicationUser : IdentityUser
{
    public ApplicationUser() => LockoutEnabled = true;

    public Guid UserId { get; set; }
}