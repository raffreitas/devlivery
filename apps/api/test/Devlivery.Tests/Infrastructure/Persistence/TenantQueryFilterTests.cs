using Devlivery.Domain.SeedWork;
using Devlivery.Infrastructure.Persistence.Context;
using Devlivery.Infrastructure.Tenancy;

using Microsoft.EntityFrameworkCore;

using Shouldly;

namespace Devlivery.Tests.Infrastructure.Persistence;

public sealed class TenantQueryFilterTests
{
    [Fact]
    public void GivenApplicationModel_WhenInspectingEntitiesWithEstablishmentId_ThenAllAreTenantScopedAndFiltered()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=unused")
            .UseSnakeCaseNamingConvention()
            .Options;
        using var dbContext = new ApplicationDbContext(options, new TenantAccessor());

        // Act
        var entitiesWithEstablishmentId = dbContext.Model.GetEntityTypes()
            .Where(entityType => entityType.FindProperty(nameof(ITenantScoped.EstablishmentId)) is not null)
            .ToList();

        // Assert
        entitiesWithEstablishmentId.ShouldNotBeEmpty();
        entitiesWithEstablishmentId.ShouldAllBe(entityType =>
            entityType.ClrType.IsAssignableTo(typeof(ITenantScoped)) &&
            entityType.GetDeclaredQueryFilters().Count != 0);
    }
}
