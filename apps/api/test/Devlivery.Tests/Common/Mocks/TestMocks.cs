using Devlivery.Infrastructure.Identity.Authentication;
using Devlivery.Infrastructure.Tenancy;

using NSubstitute;

namespace Devlivery.Tests.Common.Mocks;

public static class TestMocks
{
    public static ITenantAccessor TenantAccessor(Guid? tenantId = null)
    {
        var tenantAccessor = Substitute.For<ITenantAccessor>();
        tenantAccessor.Tenant.Returns(new Tenant(tenantId ?? Guid.NewGuid()));
        return tenantAccessor;
    }

    public static ICurrentUserAccessor CurrentUserAccessor(
        Guid? userId = null,
        string name = "Operador Teste",
        Guid? establishmentId = null)
    {
        var id = userId ?? Guid.NewGuid();
        var currentUserAccessor = Substitute.For<ICurrentUserAccessor>();
        currentUserAccessor.UserId.Returns(id);
        currentUserAccessor.GetCurrentUser().Returns(new CurrentUser(id, establishmentId ?? Guid.NewGuid()));
        currentUserAccessor.ResolveAsync(Arg.Any<CancellationToken>()).Returns(new ResolvedCurrentUser(id, name));
        return currentUserAccessor;
    }
}