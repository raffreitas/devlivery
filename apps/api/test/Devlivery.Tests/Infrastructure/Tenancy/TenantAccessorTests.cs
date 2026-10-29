using Devlivery.Infrastructure.Tenancy;

using Shouldly;

namespace Devlivery.Tests.Infrastructure.Tenancy;

public sealed class TenantAccessorTests
{
    [Fact]
    public void GivenNoRegisteredTenant_WhenReadingTenant_ThenThrowsInvalidOperationException()
    {
        // Arrange
        var tenantAccessor = new TenantAccessor();

        // Act
        var exception = Record.Exception(() => tenantAccessor.Tenant);

        // Assert
        exception.ShouldBeOfType<InvalidOperationException>();
    }

    [Fact]
    public void GivenRegisteredTenant_WhenReadingTenant_ThenReturnsIt()
    {
        // Arrange
        var tenantAccessor = new TenantAccessor();
        var tenant = new Tenant(Guid.NewGuid());
        tenantAccessor.Register(tenant);

        // Act
        var current = tenantAccessor.Tenant;

        // Assert
        current.ShouldBe(tenant);
    }
}
