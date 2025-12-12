namespace Devlivery.Shared.Infrastructure.Tenancy;

public readonly record struct Tenant(Guid Id);

public static class TenantConstants
{
    public const string TenantIdClaimType = "establishment_id";
}