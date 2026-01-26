using Devlivery.Common.Errors;
using Devlivery.Domain.Aggregates.Expenses;
using Devlivery.Domain.Aggregates.Expenses.Enums;
using Devlivery.Features.Expenses.Commands.CreateExpense;

using NSubstitute;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.CreateExpense;

[Collection("Expenses Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class CreateExpenseHandlerTests(ExpensesUnitTestFixture fixture)
{
    [Fact]
    public async Task Handle_Should_Return_NotFoundError_When_Category_Does_Not_Exist()
    {
        // Arrange
        var categoryRepository = fixture.CreateCategoryRepositoryMock();
        var expenseRepository = fixture.CreateExpenseRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        categoryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Category?)null);

        var handler = new CreateExpenseHandler(expenseRepository, categoryRepository, unitOfWork, tenantAccessor);

        var command = new CreateExpenseCommand(
            CategoryId: Guid.NewGuid(),
            Amount: 100.00m,
            DueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Supplier: "Fornecedor Teste",
            Description: "Descrição Teste",
            PaymentDate: null
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e is NotFoundError);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFoundError_When_Category_Is_Inactive()
    {
        // Arrange
        var categoryRepository = fixture.CreateCategoryRepositoryMock();
        var expenseRepository = fixture.CreateExpenseRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        var inactiveCategory = fixture.CreateCategory(isActive: false);
        categoryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(inactiveCategory);

        var handler = new CreateExpenseHandler(expenseRepository, categoryRepository, unitOfWork, tenantAccessor);

        var command = new CreateExpenseCommand(
            CategoryId: inactiveCategory.Id,
            Amount: 100.00m,
            DueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Supplier: null,
            Description: null,
            PaymentDate: null
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e is NotFoundError);
    }

    [Fact]
    public async Task Handle_Should_Create_Expense_With_Correct_Properties()
    {
        // Arrange
        var categoryRepository = fixture.CreateCategoryRepositoryMock();
        var expenseRepository = fixture.CreateExpenseRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        var category = fixture.CreateCategory(isActive: true);
        categoryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(category);

        var handler = new CreateExpenseHandler(expenseRepository, categoryRepository, unitOfWork, tenantAccessor);

        const decimal amount = 150.50m;
        var dueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        const string supplier = "Fornecedor Teste";
        const string description = "Descrição Teste";

        var command = new CreateExpenseCommand(
            CategoryId: category.Id,
            Amount: amount,
            DueDate: dueDate,
            Supplier: supplier,
            Description: description,
            PaymentDate: null
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        await expenseRepository.Received(1).AddAsync(
            Arg.Is<Expense>(e =>
                e.CategoryId == category.Id &&
                e.Amount == amount &&
                e.DueDate == dueDate &&
                e.Supplier == supplier &&
                e.Description == description &&
                e.EstablishmentId == tenantAccessor.Tenant.Id &&
                e.Status == ExpenseStatus.Pending),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_Should_Create_Expense_As_Paid_When_PaymentDate_Is_Provided()
    {
        // Arrange
        var categoryRepository = fixture.CreateCategoryRepositoryMock();
        var expenseRepository = fixture.CreateExpenseRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        var category = fixture.CreateCategory(isActive: true);
        categoryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(category);

        var handler = new CreateExpenseHandler(expenseRepository, categoryRepository, unitOfWork, tenantAccessor);

        var paymentDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var command = new CreateExpenseCommand(
            CategoryId: category.Id,
            Amount: 100.00m,
            DueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Supplier: null,
            Description: null,
            PaymentDate: paymentDate
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        await expenseRepository.Received(1).AddAsync(
            Arg.Is<Expense>(e =>
                e.Status == ExpenseStatus.Paid &&
                e.PaymentDate == paymentDate),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_Should_Use_TenantId_From_TenantAccessor()
    {
        // Arrange
        var categoryRepository = fixture.CreateCategoryRepositoryMock();
        var expenseRepository = fixture.CreateExpenseRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        var expectedTenantId = tenantAccessor.Tenant.Id;

        var category = fixture.CreateCategory(isActive: true);
        categoryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(category);

        var handler = new CreateExpenseHandler(expenseRepository, categoryRepository, unitOfWork, tenantAccessor);

        var command = new CreateExpenseCommand(
            CategoryId: category.Id,
            Amount: 100.00m,
            DueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Supplier: null,
            Description: null,
            PaymentDate: null
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        await expenseRepository.Received(1).AddAsync(
            Arg.Is<Expense>(e => e.EstablishmentId == expectedTenantId),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_Should_Return_CreateExpenseResponse_With_ExpenseId()
    {
        // Arrange
        var categoryRepository = fixture.CreateCategoryRepositoryMock();
        var expenseRepository = fixture.CreateExpenseRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        var category = fixture.CreateCategory(isActive: true);
        categoryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(category);

        var handler = new CreateExpenseHandler(expenseRepository, categoryRepository, unitOfWork, tenantAccessor);

        var command = new CreateExpenseCommand(
            CategoryId: category.Id,
            Amount: 100.00m,
            DueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Supplier: null,
            Description: null,
            PaymentDate: null
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ExpenseId.ShouldNotBe(Guid.Empty);
    }
}