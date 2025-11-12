namespace Devlivery.WebApi.Shared.Tenancy;

public readonly record struct Tenant(Guid Id);

public static class TenantConstants
{
    public const string TenantIdClaimType = "establishments_id";
}