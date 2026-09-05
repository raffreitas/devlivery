using Devlivery.Infrastructure.Tenancy;
using Devlivery.Infrastructure.Persistence.Context;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Devlivery.Infrastructure.Identity.Authentication;

public sealed class CurrentUserAccessor(
    IHttpContextAccessor httpContextAccessor,
    ITenantAccessor tenantAccessor,
    ApplicationDbContext dbContext)
    : ICurrentUserAccessor
{
    private ResolvedCurrentUser? _resolved;

    public async Task<ResolvedCurrentUser> ResolveAsync(CancellationToken cancellationToken = default)
    {
        var current = GetCurrentUser();
        if (_resolved is not null) return _resolved;
        _resolved = await dbContext.Users.AsNoTracking()
                        .Where(x => x.Id == current.Id && x.EstablishmentId == current.EstablishmentId)
                        .Select(x => new ResolvedCurrentUser(x.Id, x.Name))
                        .SingleOrDefaultAsync(cancellationToken)
                    ?? throw new UnauthorizedAccessException();
        return _resolved;
    }

    public Guid UserId => GetCurrentUser().Id;

    public CurrentUser GetCurrentUser()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
            throw new UnauthorizedAccessException();

        var user = httpContext.User;

        var userIdClaim = user.FindFirst(JwtRegisteredClaimNames.Sub);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId) || userId == Guid.Empty)
            throw new UnauthorizedAccessException();

        var establishmentId = tenantAccessor.Tenant.Id;
        if (!Guid.TryParse(user.FindFirst(TenantConstants.TenantIdClaimType)?.Value, out var tokenTenant)
            || tokenTenant == Guid.Empty || tokenTenant != establishmentId)
            throw new UnauthorizedAccessException();

        return new CurrentUser(userId, establishmentId);
    }
}