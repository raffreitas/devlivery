using Devlivery.Infrastructure.Tenancy;

using Microsoft.IdentityModel.JsonWebTokens;

namespace Devlivery.Infrastructure.Identity.Authentication;

public sealed class CurrentUserAccessor(IHttpContextAccessor httpContextAccessor, ITenantAccessor tenantAccessor)
    : ICurrentUserAccessor
{
    public Guid UserId => GetCurrentUser().Id;

    public CurrentUser GetCurrentUser()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
            throw new UnauthorizedAccessException();

        var user = httpContext.User;

        var userIdClaim = user.FindFirst(JwtRegisteredClaimNames.Sub);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
            throw new UnauthorizedAccessException();

        var establishmentId = tenantAccessor.Tenant.Id;

        return new CurrentUser(userId, establishmentId);
    }
}