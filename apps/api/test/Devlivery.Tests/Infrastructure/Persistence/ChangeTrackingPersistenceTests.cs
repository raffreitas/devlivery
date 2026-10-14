using Devlivery.Common.Events;
using Devlivery.Domain.Aggregates.CashRegister;
using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Domain.Aggregates.Establishments;
using Devlivery.Domain.Aggregates.Expenses;
using Devlivery.Domain.Aggregates.Expenses.Enums;
using Devlivery.Domain.Aggregates.Orders;
using Devlivery.Domain.Aggregates.Orders.Entities;
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

/// <summary>
/// Repositories have no Update method: aggregates they return are tracked, so changes
/// (including new and removed child entities) must be persisted by the unit of work alone.
/// </summary>
[Trait("Category", "Integration Tests")]
public sealed class ChangeTrackingPersistenceTests(PostgresDatabaseFixture database)
    : IClassFixture<PostgresDatabaseFixture>
{
    private readonly Establishment _establishment = new("Loja Teste", isActive: true);

    private Guid TenantId => _establishment.Id;

    [Fact]
    public async Task SaveChangesAsync_Should_Persist_Replaced_Order_Items_And_New_Payment()
    {
        // Arrange
        var pizza = new Product("Pizza", "Grande", 50m, "Pizzas", available: true, TenantId);
        var soda = new Product("Refrigerante", "Lata", 6m, "Bebidas", available: true, TenantId);
        var order = new Order(
            CustomerInfo.Create("Maria"),
            new DeliveryAddress("Rua A, 123", null),
            deliveryFee: 0m,
            TenantId,
            items: [new OrderItem(pizza.Id, TenantId, quantity: 1, unitPrice: 50m, notes: null)],
            payments: [new OrderPayment(TenantId, PaymentMethod.Cash, 50m)]);
        await SeedAsync(_establishment, pizza, soda, order);

        // Act
        await using (var dbContext = CreateDbContext())
        {
            var loaded = (await new OrderRepository(dbContext).GetByIdAsync(order.Id))!;
            var existingPaymentId = loaded.Payments.Single().Id;

            loaded.UpdateDetails(
                loaded.Customer,
                loaded.DeliveryAddress,
                deliveryFee: 0m,
                items: [new OrderItem(soda.Id, TenantId, quantity: 3, unitPrice: 6m, notes: "Gelado")]);
            loaded.ReconcilePayments([
                new OrderPaymentUpdate(existingPaymentId, PaymentMethod.Cash, 10m),
                new OrderPaymentUpdate(null, PaymentMethod.Pix, 8m)
            ]);

            await CreateUnitOfWork(dbContext).SaveChangesAsync();
        }

        // Assert
        await using var assertContext = CreateDbContext();
        var persisted = await assertContext.Orders
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .SingleAsync(o => o.Id == order.Id);

        persisted.Total.ShouldBe(18m);
        persisted.Items.ShouldHaveSingleItem();
        persisted.Items.Single().ProductId.ShouldBe(soda.Id);
        persisted.Items.Single().Quantity.ShouldBe(3);
        persisted.Payments.Select(p => (p.PaymentMethod, p.Amount))
            .ShouldBe([(PaymentMethod.Cash, 10m), (PaymentMethod.Pix, 8m)], ignoreOrder: true);
    }

    [Fact]
    public async Task SaveChangesAsync_Should_Persist_Subcategory_Added_To_Tracked_Category()
    {
        // Arrange
        var parent = new Category("Fornecedores", TenantId);
        await SeedAsync(_establishment, parent);

        // Act
        Guid subcategoryId;
        await using (var dbContext = CreateDbContext())
        {
            var loaded = (await new CategoryRepository(dbContext).GetByIdAsync(parent.Id))!;
            var subcategory = new Category("Bebidas", TenantId);
            loaded.AddSubcategory(subcategory);
            subcategoryId = subcategory.Id;

            await CreateUnitOfWork(dbContext).SaveChangesAsync();
        }

        // Assert
        await using var assertContext = CreateDbContext();
        var persisted = await assertContext.ExpenseCategories.SingleAsync(c => c.Id == subcategoryId);
        persisted.ParentCategoryId.ShouldBe(parent.Id);
    }

    [Fact]
    public async Task SaveChangesAsync_Should_Persist_Deposit_And_Closing_Of_Tracked_Cash_Session()
    {
        // Arrange
        var session = new CashSession(TenantId, Guid.NewGuid(), "Operador", openingAmount: 100m, notes: null);
        await SeedAsync(_establishment, session);

        // Act
        await using (var dbContext = CreateDbContext())
        {
            var loaded = (await new CashSessionRepository(dbContext).GetActiveSessionAsync())!;
            loaded.AddDeposit(50m, createdBy: Guid.NewGuid(), reason: "Reforço");
            loaded.Close(closingAmount: 150m, notes: null);

            await CreateUnitOfWork(dbContext).SaveChangesAsync();
        }

        // Assert
        await using var assertContext = CreateDbContext();
        var persisted = await assertContext.CashSessions
            .Include(s => s.Movements)
            .SingleAsync(s => s.Id == session.Id);

        persisted.Status.ShouldBe(CashSessionStatus.Closed);
        persisted.ClosingAmount.ShouldBe(150m);
        persisted.Movements.ShouldHaveSingleItem().EntryType.ShouldBe(CashSessionEntryType.Deposit);
    }

    [Fact]
    public async Task SaveChangesAsync_Should_Persist_Changes_To_Tracked_Product_And_Expense()
    {
        // Arrange
        var product = new Product("Pizza", "Grande", 50m, "Pizzas", available: true, TenantId);
        var category = new Category("Aluguel", TenantId);
        var expense = new Expense(TenantId, category.Id, 1200m, new DateOnly(2026, 10, 10));
        await SeedAsync(_establishment, product, category, expense);

        // Act
        await using (var dbContext = CreateDbContext())
        {
            var loadedProduct = (await new ProductRepository(dbContext).GetByIdAsync(product.Id))!;
            loadedProduct.Update(name: "Pizza Especial", price: 60m);
            loadedProduct.SetAsUnavailable();

            var loadedExpense = (await new ExpenseRepository(dbContext).GetByIdAsync(expense.Id))!;
            loadedExpense.MarkAsPaid(new DateOnly(2026, 10, 9));

            await CreateUnitOfWork(dbContext).SaveChangesAsync();
        }

        // Assert
        await using var assertContext = CreateDbContext();
        var persistedProduct = await assertContext.Products.SingleAsync(p => p.Id == product.Id);
        persistedProduct.Name.ShouldBe("Pizza Especial");
        persistedProduct.Price.ShouldBe(60m);
        persistedProduct.Available.ShouldBeFalse();

        var persistedExpense = await assertContext.Expenses.SingleAsync(e => e.Id == expense.Id);
        persistedExpense.Status.ShouldBe(ExpenseStatus.Paid);
        persistedExpense.PaymentDate.ShouldBe(new DateOnly(2026, 10, 9));
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