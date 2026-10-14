using System.Data.Common;

using Devlivery.Common.Time;
using Devlivery.Domain.Aggregates.Establishments;
using Devlivery.Domain.Aggregates.Expenses;
using Devlivery.Features.Expenses.Queries.GetAllExpenses;
using Devlivery.Infrastructure.Persistence.Abstractions;
using Devlivery.Infrastructure.Persistence.Context;
using Devlivery.Infrastructure.Tenancy;
using Devlivery.Tests.Common.Builders;
using Devlivery.Tests.Common.Mocks;
using Devlivery.Tests.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

using Npgsql;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Queries.GetAllExpenses;

[Trait("Category", "Integration Tests")]
public sealed class GetAllExpensesHandlerTests(PostgresDatabaseFixture database)
    : IClassFixture<PostgresDatabaseFixture>, IAsyncLifetime
{
    private static readonly DateOnly Today = new(2026, 4, 17);

    private readonly Establishment _establishment = new EstablishmentBuilder().Build();
    private Category _parentCategory = null!;
    private Category _subcategory = null!;

    private Guid TenantId => _establishment.Id;

    public async Task InitializeAsync()
    {
        _parentCategory = new CategoryBuilder().WithEstablishmentId(TenantId).WithName("Operacional").Build();
        _subcategory = new CategoryBuilder().WithEstablishmentId(TenantId).WithName("Insumos")
            .WithParent(_parentCategory).Build();
        await SeedAsync(_establishment, _parentCategory, _subcategory);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GivenPendingExpensesWithSameDueDate_WhenListing_ThenOrdersByCreationDate()
    {
        // Arrange
        var dueDate = Today.AddDays(5);
        await SeedExpenseAsync("Mais nova", new ExpenseBuilder().WithDueDate(dueDate), new DateTime(2026, 4, 16));
        await SeedExpenseAsync("Mais antiga", new ExpenseBuilder().WithDueDate(dueDate), new DateTime(2026, 4, 10));
        await SeedExpenseAsync("Intermediária", new ExpenseBuilder().WithDueDate(dueDate), new DateTime(2026, 4, 12));

        // Act
        var result = await CreateHandler().HandleAsync(new GetAllExpensesQuery(), CancellationToken.None);

        // Assert
        result.Value.Select(e => e.Description).ShouldBe(["Mais antiga", "Intermediária", "Mais nova"]);
    }

    [Fact]
    public async Task GivenExpensesInEveryStatus_WhenListing_ThenOrdersByOperationalPriorityThenDueDate()
    {
        // Arrange
        await SeedExpenseAsync("Pago", new ExpenseBuilder().WithDueDate(Today.AddDays(-10)).AsPaid(Today.AddDays(-9)));
        await SeedExpenseAsync("Pendente mais próximo", new ExpenseBuilder().WithDueDate(Today.AddDays(1)));
        await SeedExpenseAsync("Vence hoje", new ExpenseBuilder().WithDueDate(Today));
        await SeedExpenseAsync("Vencida", new ExpenseBuilder().WithDueDate(Today.AddDays(-2)));
        await SeedExpenseAsync("Pendente mais distante", new ExpenseBuilder().WithDueDate(Today.AddDays(4)));
        await SeedExpenseAsync("Cancelada", new ExpenseBuilder().WithDueDate(Today.AddDays(-20)).AsCancelled());

        // Act
        var result = await CreateHandler().HandleAsync(new GetAllExpensesQuery(), CancellationToken.None);

        // Assert
        result.Value.Select(e => (e.Description, e.Status)).ShouldBe([
            ("Vencida", ExpenseDisplayStatus.Overdue),
            ("Vence hoje", ExpenseDisplayStatus.DueToday),
            ("Pendente mais próximo", ExpenseDisplayStatus.Pending),
            ("Pendente mais distante", ExpenseDisplayStatus.Pending),
            ("Pago", ExpenseDisplayStatus.Paid),
            ("Cancelada", ExpenseDisplayStatus.Cancelled)
        ]);
    }

    [Fact]
    public async Task GivenPaidExpenses_WhenListing_ThenOrdersThemByMostRecentPaymentRegardlessOfDueDate()
    {
        // Arrange
        await SeedExpenseAsync("Pago antigo", new ExpenseBuilder().WithDueDate(Today.AddDays(-10)).AsPaid(Today.AddDays(-90)));
        await SeedExpenseAsync("Pago recente", new ExpenseBuilder().WithDueDate(Today.AddDays(-200)).AsPaid(Today.AddDays(-1)));
        await SeedExpenseAsync("Pendente", new ExpenseBuilder().WithDueDate(Today.AddDays(2)));

        // Act
        var result = await CreateHandler().HandleAsync(new GetAllExpensesQuery(), CancellationToken.None);

        // Assert
        result.Value.Select(e => e.Description).ShouldBe(["Pendente", "Pago recente", "Pago antigo"]);
    }

    [Theory]
    [InlineData(ExpenseDisplayStatus.Overdue, "Vencida")]
    [InlineData(ExpenseDisplayStatus.DueToday, "Vence hoje")]
    [InlineData(ExpenseDisplayStatus.Pending, "Pendente")]
    [InlineData(ExpenseDisplayStatus.Paid, "Paga")]
    [InlineData(ExpenseDisplayStatus.Cancelled, "Cancelada")]
    public async Task GivenStatusFilter_WhenListing_ThenReturnsOnlyExpensesInThatDisplayStatus(
        ExpenseDisplayStatus status, string expectedDescription)
    {
        // Arrange
        await SeedOneExpensePerDisplayStatusAsync();

        // Act
        var result = await CreateHandler().HandleAsync(new GetAllExpensesQuery(Status: status), CancellationToken.None);

        // Assert
        result.Value.ShouldHaveSingleItem().Description.ShouldBe(expectedDescription);
    }

    [Fact]
    public async Task GivenParentCategoryFilter_WhenListing_ThenIncludesExpensesFiledDirectlyUnderParent()
    {
        // Arrange
        await SeedExpenseAsync("Na subcategoria", new ExpenseBuilder());
        await SeedExpenseAsync("Direto na categoria", new ExpenseBuilder(), categoryId: _parentCategory.Id);

        // Act
        var result = await CreateHandler().HandleAsync(new GetAllExpensesQuery(CategoryId: _parentCategory.Id),
            CancellationToken.None);

        // Assert
        result.Value.Select(e => e.Description).ShouldBe(["Na subcategoria", "Direto na categoria"],
            ignoreOrder: true);
        result.Value.Single(e => e.Description == "Direto na categoria").Category.Subcategories.ShouldBeEmpty();
    }

    [Fact]
    public async Task GivenParentCategoryFilter_WhenListing_ThenReturnsSubcategoryExpensesNestedUnderParent()
    {
        // Arrange
        var otherParent = new CategoryBuilder().WithEstablishmentId(TenantId).WithName("Pessoal").Build();
        var otherSubcategory = new CategoryBuilder().WithEstablishmentId(TenantId).WithName("Salários")
            .WithParent(otherParent).Build();
        await SeedAsync(otherParent, otherSubcategory);
        await SeedExpenseAsync("Insumo", new ExpenseBuilder());
        await SeedExpenseAsync("Salário", new ExpenseBuilder(), categoryId: otherSubcategory.Id);

        // Act
        var result = await CreateHandler().HandleAsync(new GetAllExpensesQuery(CategoryId: _parentCategory.Id),
            CancellationToken.None);

        // Assert
        var expense = result.Value.ShouldHaveSingleItem();
        expense.Description.ShouldBe("Insumo");
        expense.Category.Id.ShouldBe(_parentCategory.Id);
        expense.Category.Name.ShouldBe("Operacional");
        expense.Category.Subcategories.ShouldHaveSingleItem().Name.ShouldBe("Insumos");
    }

    [Fact]
    public async Task GivenDueDateRange_WhenListing_ThenReturnsExpensesDueWithinInclusiveBounds()
    {
        // Arrange
        await SeedExpenseAsync("Antes", new ExpenseBuilder().WithDueDate(new DateOnly(2026, 4, 30)));
        await SeedExpenseAsync("Início", new ExpenseBuilder().WithDueDate(new DateOnly(2026, 5, 1)));
        await SeedExpenseAsync("Fim", new ExpenseBuilder().WithDueDate(new DateOnly(2026, 5, 31)));
        await SeedExpenseAsync("Depois", new ExpenseBuilder().WithDueDate(new DateOnly(2026, 6, 1)));
        var query = new GetAllExpensesQuery(StartDate: new DateOnly(2026, 5, 1), EndDate: new DateOnly(2026, 5, 31));

        // Act
        var result = await CreateHandler().HandleAsync(query, CancellationToken.None);

        // Assert
        result.Value.Select(e => e.Description).ShouldBe(["Início", "Fim"]);
    }

    [Fact]
    public async Task GivenExpensesOfAnotherTenant_WhenListing_ThenReturnsOnlyCurrentTenantExpenses()
    {
        // Arrange
        var otherEstablishment = new EstablishmentBuilder().Build();
        var otherCategory = new CategoryBuilder().WithEstablishmentId(otherEstablishment.Id).Build();
        var otherExpense = new ExpenseBuilder().WithEstablishmentId(otherEstablishment.Id)
            .WithCategoryId(otherCategory.Id).WithDescription("De outra loja").Build();
        await SeedAsync(otherEstablishment, otherCategory, otherExpense);
        await SeedExpenseAsync("Da loja atual", new ExpenseBuilder());

        // Act
        var result = await CreateHandler().HandleAsync(new GetAllExpensesQuery(), CancellationToken.None);

        // Assert
        result.Value.ShouldHaveSingleItem().Description.ShouldBe("Da loja atual");
    }

    private GetAllExpensesHandler CreateHandler()
    {
        var noonInBrazil = Today.ToDateTime(new TimeOnly(12, 0));
        var timeProvider = new FakeTimeProvider(TimeZoneInfo.ConvertTimeToUtc(noonInBrazil, BrazilTime.TimeZone));

        return new GetAllExpensesHandler(
            new TestDbConnectionFactory(database.ConnectionString),
            TestMocks.TenantAccessor(TenantId),
            timeProvider);
    }

    private async Task SeedOneExpensePerDisplayStatusAsync()
    {
        await SeedExpenseAsync("Vencida", new ExpenseBuilder().WithDueDate(Today.AddDays(-1)));
        await SeedExpenseAsync("Vence hoje", new ExpenseBuilder().WithDueDate(Today));
        await SeedExpenseAsync("Pendente", new ExpenseBuilder().WithDueDate(Today.AddDays(1)));
        await SeedExpenseAsync("Paga", new ExpenseBuilder().WithDueDate(Today.AddDays(-1)).AsPaid(Today));
        await SeedExpenseAsync("Cancelada", new ExpenseBuilder().WithDueDate(Today.AddDays(-1)).AsCancelled());
    }

    private async Task SeedExpenseAsync(string description, ExpenseBuilder builder, DateTime? createdAt = null,
        Guid? categoryId = null)
    {
        var expense = builder
            .WithEstablishmentId(TenantId)
            .WithCategoryId(categoryId ?? _subcategory.Id)
            .WithDescription(description)
            .Build();
        await SeedAsync(expense);

        if (createdAt is not null)
        {
            await using var dbContext = CreateDbContext();
            var createdAtUtc = DateTime.SpecifyKind(createdAt.Value, DateTimeKind.Utc);
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"update expenses set created_at = {createdAtUtc} where id = {expense.Id}");
        }
    }

    private async Task SeedAsync(params object[] entities)
    {
        await using var dbContext = CreateDbContext();
        dbContext.AddRange(entities);
        await dbContext.SaveChangesAsync();
    }

    private ApplicationDbContext CreateDbContext()
    {
        var tenantAccessor = new TenantAccessor();
        tenantAccessor.Register(new Tenant(TenantId));
        return new ApplicationDbContext(database.CreateOptions(), tenantAccessor);
    }

    private sealed class TestDbConnectionFactory(string connectionString) : IDbConnectionFactory
    {
        public async ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
        {
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
    }
}