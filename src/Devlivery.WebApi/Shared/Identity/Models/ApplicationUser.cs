using Microsoft.AspNetCore.Identity;

namespace Devlivery.WebApi.Shared.Identity.Models;

public sealed class ApplicationUser : IdentityUser
{
    public Guid UserId { get; set; }
}