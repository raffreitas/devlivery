using Bogus;

using Devlivery.Features.Expenses.Domain.Aggregates.Categories;
using Devlivery.Features.Expenses.Domain.Aggregates.Expenses;
using Devlivery.Features.Expenses.Domain.Aggregates.Expenses.Enums;
using Devlivery.Shared.Infrastructure.Persistence;
using Devlivery.Shared.Infrastructure.Tenancy;

using NSubstitute;

namespace Devlivery.Tests.Features.Expenses;

/// <summary>
/// Fixture para testes de unidade da feature Expenses.
/// Fornece factory methods para criar mocks das dependências utilizando NSubstitute.
/// </summary>
public sealed class ExpensesUnitTestFixture : IDisposable
{
    public Faker Faker { get; } = new("pt_BR");

    private readonly Guid _defaultTenantId = Guid.NewGuid();

    /// <summary>
    /// Cria um mock de ITenantAccessor com um tenant padrão.
    /// </summary>
    public ITenantAccessor CreateTenantAccessorMock(Guid? tenantId = null)
    {
        var mock = Substitute.For<ITenantAccessor>();
        var tenant = new Tenant(tenantId ?? _defaultTenantId);
        mock.Tenant.Returns(tenant);
        return mock;
    }

    /// <summary>
    /// Cria um mock de IExpenseRepository.
    /// </summary>
    public IExpenseRepository CreateExpenseRepositoryMock()
    {
        return Substitute.For<IExpenseRepository>();
    }

    /// <summary>
    /// Cria um mock de ICategoryRepository.
    /// </summary>
    public ICategoryRepository CreateCategoryRepositoryMock()
    {
        return Substitute.For<ICategoryRepository>();
    }

    /// <summary>
    /// Cria um mock de IUnitOfWork.
    /// </summary>
    public IUnitOfWork CreateUnitOfWorkMock()
    {
        return Substitute.For<IUnitOfWork>();
    }

    /// <summary>
    /// Cria uma instância de Expense para uso em testes.
    /// </summary>
    public Expense CreateExpense(
        Guid? categoryId = null,
        Guid? establishmentId = null,
        decimal? amount = null,
        DateOnly? dueDate = null,
        string? supplier = null,
        string? description = null,
        DateOnly? paymentDate = null,
        ExpenseStatus? status = null)
    {
        var expense = new Expense(
            establishmentId: establishmentId ?? _defaultTenantId,
            categoryId: categoryId ?? Guid.NewGuid(),
            amount: amount ?? Faker.Random.Decimal(10, 1000),
            dueDate: dueDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            supplier: supplier,
            description: description,
            paymentDate: paymentDate
        );

        // Ajustar status se necessário (após criação)
        if (status == ExpenseStatus.Cancelled && expense.Status != ExpenseStatus.Cancelled)
        {
            expense.Cancel();
        }

        return expense;
    }

    /// <summary>
    /// Cria uma instância de Category para uso em testes.
    /// </summary>
    public Category CreateCategory(
        string? name = null,
        Guid? establishmentId = null,
        bool? isActive = null)
    {
        var category = new Category(
            name: name ?? Faker.Commerce.Categories(1)[0],
            establishmentId: establishmentId ?? _defaultTenantId
        );

        if (isActive == false)
        {
            category.Deactivate();
        }

        return category;
    }

    public void Dispose()
    {
        // Cleanup se necessário
    }
}

[CollectionDefinition("Expenses Unit Tests")]
public sealed class ExpensesUnitTestCollection : ICollectionFixture<ExpensesUnitTestFixture>;

