using System.Data.Common;

using Dapper;

using Devlivery.Domain.Aggregates.Expenses.Enums;
using Devlivery.Features.Expenses.Queries.GetAllExpenses;
using Devlivery.Infrastructure.Persistence.Abstractions;
using Devlivery.Infrastructure.Tenancy;
using Devlivery.Infrastructure.Time.Abstractions;

using Npgsql;

using NSubstitute;

using Shouldly;

using Testcontainers.PostgreSql;

namespace Devlivery.Tests.Features.Expenses.Queries.GetAllExpenses;

[Collection("Expenses Unit Tests")]
[Trait("Category", "Integration Tests")]
public sealed class GetAllExpensesHandlerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:16-alpine")
        .WithCleanUp(true)
        .Build();

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task Handle_MultipleExpensesShareStatusAndDueDate_OrdersByCreatedAtAscending()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var today = new DateOnly(2026, 4, 17);

        await ResetDatabaseAsync();

        var childCategoryId = await SeedCategoriesAsync(tenantId);
        var dueDate = today.AddDays(5);

        await SeedExpenseAsync(CreateExpenseRow(
            tenantId,
            childCategoryId,
            description: "Despesa mais nova",
            dueDate: dueDate,
            status: nameof(ExpenseStatus.Pending),
            createdAt: new DateTime(2026, 4, 16, 10, 0, 0)));
        await SeedExpenseAsync(CreateExpenseRow(
            tenantId,
            childCategoryId,
            description: "Despesa mais antiga",
            dueDate: dueDate,
            status: nameof(ExpenseStatus.Pending),
            createdAt: new DateTime(2026, 4, 10, 10, 0, 0)));
        await SeedExpenseAsync(CreateExpenseRow(
            tenantId,
            childCategoryId,
            description: "Despesa intermediaria",
            dueDate: dueDate,
            status: nameof(ExpenseStatus.Pending),
            createdAt: new DateTime(2026, 4, 12, 10, 0, 0)));

        var handler = CreateHandler(tenantId, today);

        // Act
        var result = await handler.Handle(new GetAllExpensesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Select(expense => expense.Description).ShouldBe([
            "Despesa mais antiga",
            "Despesa intermediaria",
            "Despesa mais nova"
        ]);
    }

    [Fact]
    public async Task Handle_ExpensesHaveDifferentDisplayStatuses_OrdersByOperationalPriorityThenDueDate()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var today = new DateOnly(2026, 4, 17);

        await ResetDatabaseAsync();

        var childCategoryId = await SeedCategoriesAsync(tenantId);

        await SeedExpenseAsync(CreateExpenseRow(
            tenantId,
            childCategoryId,
            description: "Pago",
            dueDate: today.AddDays(-10),
            status: nameof(ExpenseStatus.Paid),
            createdAt: new DateTime(2026, 4, 1, 8, 0, 0),
            paymentDate: today.AddDays(-9)));
        await SeedExpenseAsync(CreateExpenseRow(
            tenantId,
            childCategoryId,
            description: "Pendente mais proximo",
            dueDate: today.AddDays(1),
            status: nameof(ExpenseStatus.Pending),
            createdAt: new DateTime(2026, 4, 12, 8, 0, 0)));
        await SeedExpenseAsync(CreateExpenseRow(
            tenantId,
            childCategoryId,
            description: "Vence hoje",
            dueDate: today,
            status: nameof(ExpenseStatus.Pending),
            createdAt: new DateTime(2026, 4, 11, 8, 0, 0)));
        await SeedExpenseAsync(CreateExpenseRow(
            tenantId,
            childCategoryId,
            description: "Vencida",
            dueDate: today.AddDays(-2),
            status: nameof(ExpenseStatus.Pending),
            createdAt: new DateTime(2026, 4, 10, 8, 0, 0)));
        await SeedExpenseAsync(CreateExpenseRow(
            tenantId,
            childCategoryId,
            description: "Pendente mais distante",
            dueDate: today.AddDays(4),
            status: nameof(ExpenseStatus.Pending),
            createdAt: new DateTime(2026, 4, 13, 8, 0, 0)));
        await SeedExpenseAsync(CreateExpenseRow(
            tenantId,
            childCategoryId,
            description: "Cancelada",
            dueDate: today.AddDays(-20),
            status: nameof(ExpenseStatus.Cancelled),
            createdAt: new DateTime(2026, 4, 2, 8, 0, 0)));

        var handler = CreateHandler(tenantId, today);

        // Act
        var result = await handler.Handle(new GetAllExpensesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Select(expense => expense.Description).ShouldBe([
            "Vencida",
            "Vence hoje",
            "Pendente mais proximo",
            "Pendente mais distante",
            "Pago",
            "Cancelada"
        ]);
        result.Value.Select(expense => expense.Status).ShouldBe([
            ExpenseDisplayStatus.Overdue,
            ExpenseDisplayStatus.DueToday,
            ExpenseDisplayStatus.Pending,
            ExpenseDisplayStatus.Pending,
            ExpenseDisplayStatus.Paid,
            ExpenseDisplayStatus.Cancelled
        ]);
    }

    [Fact]
    public async Task Handle_ExpensesArePaidOrCancelled_KeepsCompletedItemsAfterPendingItems()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var today = new DateOnly(2026, 4, 17);

        await ResetDatabaseAsync();

        var childCategoryId = await SeedCategoriesAsync(tenantId);

        await SeedExpenseAsync(CreateExpenseRow(
            tenantId,
            childCategoryId,
            description: "Despesa paga antiga",
            dueDate: today.AddDays(-15),
            status: nameof(ExpenseStatus.Paid),
            createdAt: new DateTime(2026, 4, 1, 10, 0, 0),
            paymentDate: today.AddDays(-14)));
        await SeedExpenseAsync(CreateExpenseRow(
            tenantId,
            childCategoryId,
            description: "Despesa cancelada antiga",
            dueDate: today.AddDays(-12),
            status: nameof(ExpenseStatus.Cancelled),
            createdAt: new DateTime(2026, 4, 2, 10, 0, 0)));
        await SeedExpenseAsync(CreateExpenseRow(
            tenantId,
            childCategoryId,
            description: "Despesa pendente futura",
            dueDate: today.AddDays(7),
            status: nameof(ExpenseStatus.Pending),
            createdAt: new DateTime(2026, 4, 3, 10, 0, 0)));

        var handler = CreateHandler(tenantId, today);

        // Act
        var result = await handler.Handle(new GetAllExpensesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Select(expense => expense.Description).ShouldBe([
            "Despesa pendente futura",
            "Despesa paga antiga",
            "Despesa cancelada antiga"
        ]);
    }

    [Fact]
    public async Task Handle_PaidExpenses_AppearAfterNonPaid_OrderedByPaymentDateDesc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var today = new DateOnly(2026, 4, 17);

        await ResetDatabaseAsync();

        var childCategoryId = await SeedCategoriesAsync(tenantId);

        // Paid with old due date, recent payment
        await SeedExpenseAsync(CreateExpenseRow(
            tenantId,
            childCategoryId,
            description: "Pago recente, vencimento antigo",
            dueDate: today.AddDays(-100),
            status: nameof(ExpenseStatus.Paid),
            createdAt: new DateTime(2026, 1, 1, 10, 0, 0),
            paymentDate: today.AddDays(-1)));

        // Paid with old due date, old payment
        await SeedExpenseAsync(CreateExpenseRow(
            tenantId,
            childCategoryId,
            description: "Pago antigo, vencimento antigo",
            dueDate: today.AddDays(-200),
            status: nameof(ExpenseStatus.Paid),
            createdAt: new DateTime(2025, 12, 1, 10, 0, 0),
            paymentDate: today.AddDays(-90)));

        // Pending, due soon
        await SeedExpenseAsync(CreateExpenseRow(
            tenantId,
            childCategoryId,
            description: "Pendente, vence em breve",
            dueDate: today.AddDays(2),
            status: nameof(ExpenseStatus.Pending),
            createdAt: new DateTime(2026, 4, 10, 10, 0, 0)));

        // Pending, overdue
        await SeedExpenseAsync(CreateExpenseRow(
            tenantId,
            childCategoryId,
            description: "Pendente, vencida",
            dueDate: today.AddDays(-2),
            status: nameof(ExpenseStatus.Pending),
            createdAt: new DateTime(2026, 4, 5, 10, 0, 0)));

        var handler = CreateHandler(tenantId, today);

        // Act
        var result = await handler.Handle(new GetAllExpensesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        // Paid never appears before non-paid, and paid are ordered by PaymentDate desc
        result.Value.Select(e => e.Description).ShouldBe([
            "Pendente, vencida",
            "Pendente, vence em breve",
            "Pago recente, vencimento antigo",
            "Pago antigo, vencimento antigo"
        ]);
        result.Value.Select(e => e.Status).ShouldBe([
            ExpenseDisplayStatus.Overdue,
            ExpenseDisplayStatus.Pending,
            ExpenseDisplayStatus.Paid,
            ExpenseDisplayStatus.Paid
        ]);
    }

    private GetAllExpensesHandler CreateHandler(Guid tenantId, DateOnly today)
    {
        var tenantAccessor = Substitute.For<ITenantAccessor>();
        tenantAccessor.Tenant.Returns(new Tenant(tenantId));

        var dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.GetLocalDate().Returns(today);

        return new GetAllExpensesHandler(
            new TestDbConnectionFactory(_postgresContainer.GetConnectionString()),
            tenantAccessor,
            dateTimeProvider);
    }

    private async Task ResetDatabaseAsync()
    {
        const string sql =
            """
            drop table if exists public.expenses;
            drop table if exists public.expense_categories;

            create table public.expense_categories
            (
                id uuid primary key,
                establishment_id uuid not null,
                name text not null,
                is_active boolean not null,
                parent_category_id uuid null references public.expense_categories (id)
            );

            create table public.expenses
            (
                id uuid primary key,
                establishment_id uuid not null,
                category_id uuid not null references public.expense_categories (id),
                supplier text null,
                description text null,
                amount numeric not null,
                due_date date not null,
                payment_date date null,
                status text not null,
                created_at timestamp without time zone not null,
                updated_at timestamp without time zone not null
            );
            """;

        await using var connection = new NpgsqlConnection(_postgresContainer.GetConnectionString());
        await connection.OpenAsync();
        await connection.ExecuteAsync(sql);
    }

    private async Task<Guid> SeedCategoriesAsync(Guid tenantId)
    {
        var parentCategoryId = Guid.NewGuid();
        var childCategoryId = Guid.NewGuid();

        const string sql =
            """
            insert into public.expense_categories (id, establishment_id, name, is_active, parent_category_id)
            values (@ParentCategoryId, @TenantId, 'Operacional', true, null),
                   (@ChildCategoryId, @TenantId, 'Insumos', true, @ParentCategoryId);
            """;

        await using var connection = new NpgsqlConnection(_postgresContainer.GetConnectionString());
        await connection.OpenAsync();
        await connection.ExecuteAsync(sql, new
        {
            ParentCategoryId = parentCategoryId,
            ChildCategoryId = childCategoryId,
            TenantId = tenantId
        });

        return childCategoryId;
    }

    private async Task SeedExpenseAsync(ExpenseRow expense)
    {
        const string sql =
            """
            insert into public.expenses
                (id, establishment_id, category_id, supplier, description, amount, due_date, payment_date, status, created_at, updated_at)
            values
                (@Id, @EstablishmentId, @CategoryId, @Supplier, @Description, @Amount, @DueDate, @PaymentDate, @Status, @CreatedAt, @UpdatedAt);
            """;

        var parameters = new DynamicParameters();
        parameters.Add("Id", expense.Id, System.Data.DbType.Guid);
        parameters.Add("EstablishmentId", expense.EstablishmentId, System.Data.DbType.Guid);
        parameters.Add("CategoryId", expense.CategoryId, System.Data.DbType.Guid);
        parameters.Add("Supplier", expense.Supplier, System.Data.DbType.String);
        parameters.Add("Description", expense.Description, System.Data.DbType.String);
        parameters.Add("Amount", expense.Amount, System.Data.DbType.Decimal);
        parameters.Add("DueDate", expense.DueDate, System.Data.DbType.Date);
        parameters.Add("PaymentDate", expense.PaymentDate, System.Data.DbType.Date);
        parameters.Add("Status", expense.Status, System.Data.DbType.String);
        parameters.Add("CreatedAt", expense.CreatedAt, System.Data.DbType.DateTime);
        parameters.Add("UpdatedAt", expense.UpdatedAt, System.Data.DbType.DateTime);

        await using var connection = new NpgsqlConnection(_postgresContainer.GetConnectionString());
        await connection.OpenAsync();
        await connection.ExecuteAsync(sql, parameters);
    }

    private static ExpenseRow CreateExpenseRow(
        Guid tenantId,
        Guid categoryId,
        string description,
        DateOnly dueDate,
        string status,
        DateTime createdAt,
        DateOnly? paymentDate = null,
        string supplier = "Fornecedor teste",
        decimal amount = 100m)
    {
        var normalizedCreatedAt = DateTime.SpecifyKind(createdAt, DateTimeKind.Utc);

        return
        new(
            Guid.NewGuid(),
            tenantId,
            categoryId,
            supplier,
            description,
            amount,
            dueDate,
            paymentDate,
            status,
            normalizedCreatedAt,
            normalizedCreatedAt.AddMinutes(5));
    }

    private sealed record ExpenseRow(
        Guid Id,
        Guid EstablishmentId,
        Guid CategoryId,
        string Supplier,
        string Description,
        decimal Amount,
        DateOnly DueDate,
        DateOnly? PaymentDate,
        string Status,
        DateTime CreatedAt,
        DateTime UpdatedAt);

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