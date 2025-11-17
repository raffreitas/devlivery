using Microsoft.AspNetCore.Identity;

namespace Devlivery.WebApi.Shared.Identity.Users.Models;

public sealed class ApplicationUser : IdentityUser
{
    public Guid UserId { get; set; }
}