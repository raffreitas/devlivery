namespace Devlivery.Infrastructure.Tenancy;

public interface ITenantAccessor
{
    Tenant Tenant { get; }
    void Register(Tenant tenant);
}

public sealed class TenantAccessor : ITenantAccessor
{
    private Tenant? _tenant;

    public Tenant Tenant =>
        _tenant ?? throw new InvalidOperationException("No tenant has been registered for the current scope.");

    public void Register(Tenant tenant) => _tenant = tenant;
}