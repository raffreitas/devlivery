namespace Devlivery.Shared.Infrastructure.Tenancy;

public interface ITenantLocator
{
    Task<Tenant> GetAsync(CancellationToken cancellationToken = default);
}

public sealed class TenantLocator(IHttpContextAccessor httpContextAccessor) : ITenantLocator
{
    public Task<Tenant> GetAsync(CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
            throw new InvalidOperationException("No HTTP context available.");

        var user = httpContext.User;
        if (user.Identity is null || !user.Identity.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var tenantIdClaim = user.Claims.FirstOrDefault(c => c.Type == TenantConstants.TenantIdClaimType);

        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var parsedTenantId))
            throw new UnauthorizedAccessException("Tenant information is missing in the token.");

        var tenant = new Tenant(parsedTenantId);
        return Task.FromResult(tenant);
    }
}