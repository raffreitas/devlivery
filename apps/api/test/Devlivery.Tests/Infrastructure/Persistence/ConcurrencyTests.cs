using Devlivery.Common.Events;
using Devlivery.Domain.Aggregates.CashRegister;
using Devlivery.Domain.Aggregates.Establishments;
using Devlivery.Domain.Aggregates.Orders;
using Devlivery.Domain.Aggregates.Orders.Entities;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.Aggregates.Orders.ValueObjects;
using Devlivery.Domain.Aggregates.Products;
using Devlivery.Domain.Common.Enums;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Infrastructure.Persistence.Context;
using Devlivery.Infrastructure.Persistence.Repositories;
using Devlivery.Infrastructure.Tenancy;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shouldly;

namespace Devlivery.Tests.Infrastructure.Persistence;

[Trait("Category", "Integration Tests")]
public sealed class ConcurrencyTests(PostgresDatabaseFixture database)
    : IClassFixture<PostgresDatabaseFixture>
{
    private readonly Establishment _establishment = new("Loja Teste", isActive: true);

    private Guid TenantId => _establishment.Id;

    [Fact]
    public async Task GivenCashSessionClosedByAnotherContext_WhenClosingStaleCopy_ThenThrowsConcurrencyException()
    {
        // Arrange
        var session = new CashSession(TenantId, Guid.NewGuid(), "Operador", openingAmount: 100m, notes: null);
        await SeedAsync(_establishment, session);

        await using var firstContext = CreateDbContext();
        await using var secondContext = CreateDbContext();
        var first = (await new CashSessionRepository(firstContext).GetActiveSessionAsync())!;
        var second = (await new CashSessionRepository(secondContext).GetActiveSessionAsync())!;

        first.Close(closingAmount: 100m, notes: null);
        await CreateUnitOfWork(firstContext).CommitAsync();

        second.Close(closingAmount: 90m, notes: null);

        // Act
        var exception = await Record.ExceptionAsync(() => CreateUnitOfWork(secondContext).CommitAsync());

        // Assert
        exception.ShouldBeOfType<DbUpdateConcurrencyException>();
        await using var assertContext = CreateDbContext();
        var persisted = await assertContext.CashSessions.SingleAsync(s => s.Id == session.Id);
        persisted.ClosingAmount.ShouldBe(100m);
    }

    [Fact]
    public async Task GivenOrderStatusChangedByAnotherContext_WhenChangingStaleCopy_ThenThrowsConcurrencyException()
    {
        // Arrange
        var product = new Product("Pizza", "Grande", 50m, "Pizzas", available: true, TenantId);
        var order = new Order(
            CustomerInfo.Create("Maria"),
            new DeliveryAddress("Rua A, 123", null),
            deliveryFee: 0m,
            TenantId,
            items: [new OrderItem(product.Id, TenantId, quantity: 1, unitPrice: 50m, notes: null)],
            payments: [new OrderPayment(TenantId, PaymentMethod.Cash, 50m)]);
        await SeedAsync(_establishment, product, order);

        await using var firstContext = CreateDbContext();
        await using var secondContext = CreateDbContext();
        var first = (await new OrderRepository(firstContext).GetByIdAsync(order.Id))!;
        var second = (await new OrderRepository(secondContext).GetByIdAsync(order.Id))!;

        first.UpdateStatus(OrderStatus.Preparing);
        await CreateUnitOfWork(firstContext).CommitAsync();

        second.UpdateStatus(OrderStatus.Canceled);

        // Act
        var exception = await Record.ExceptionAsync(() => CreateUnitOfWork(secondContext).CommitAsync());

        // Assert
        exception.ShouldBeOfType<DbUpdateConcurrencyException>();
        await using var assertContext = CreateDbContext();
        var persisted = await assertContext.Orders.SingleAsync(o => o.Id == order.Id);
        persisted.Status.ShouldBe(OrderStatus.Preparing);
    }

    private ApplicationDbContext CreateDbContext()
    {
        var tenantAccessor = new TenantAccessor();
        tenantAccessor.Register(new Tenant(TenantId));
        return new ApplicationDbContext(database.CreateOptions(), tenantAccessor);
    }

    private static UnitOfWork CreateUnitOfWork(ApplicationDbContext dbContext) =>
        new(dbContext, new DomainEventDispatcher(new ServiceCollection().BuildServiceProvider()));

    private async Task SeedAsync(params object[] entities)
    {
        await using var dbContext = CreateDbContext();
        dbContext.AddRange(entities);
        await dbContext.SaveChangesAsync();
    }
}
