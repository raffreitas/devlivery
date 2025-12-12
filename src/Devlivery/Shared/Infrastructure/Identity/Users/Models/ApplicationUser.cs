using Microsoft.AspNetCore.Identity;

namespace Devlivery.Shared.Infrastructure.Identity.Users.Models;

public sealed class ApplicationUser : IdentityUser
{
    public Guid UserId { get; set; }
}