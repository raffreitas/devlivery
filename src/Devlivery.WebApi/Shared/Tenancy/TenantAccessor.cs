namespace Devlivery.WebApi.Shared.Tenancy;

public interface ITenantAccessor
{
    Tenant Tenant { get; }
    void Register(Tenant tenant);
}

public sealed class TenantAccessor(IHttpContextAccessor httpContextAccessor) : ITenantAccessor
{
    public Tenant Tenant { get; private set; }

    public void Register(Tenant tenant) => Tenant = tenant;
}