using Devlivery.Features.Expenses.Commands.UpdateExpense;
using Devlivery.Features.Expenses.Domain.Aggregates.Categories;
using Devlivery.Features.Expenses.Domain.Aggregates.Expenses;

using NSubstitute;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.UpdateExpense;

[Collection("Expenses Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class UpdateExpenseHandlerTests(ExpensesUnitTestFixture fixture)
{
    [Fact]
    public async Task Handle_Should_Return_Fail_When_Expense_Does_Not_Exist()
    {
        // Arrange
        var expenseRepository = fixture.CreateExpenseRepositoryMock();
        var categoryRepository = fixture.CreateCategoryRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        expenseRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Expense?)null);

        var handler = new UpdateExpenseHandler(expenseRepository, categoryRepository, unitOfWork);

        var command = new UpdateExpenseCommand(
            ExpenseId: Guid.NewGuid(),
            CategoryId: null,
            Amount: 200.00m,
            DueDate: null,
            Supplier: null,
            Description: null
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_Should_Return_Fail_When_Category_Does_Not_Exist()
    {
        // Arrange
        var expenseRepository = fixture.CreateExpenseRepositoryMock();
        var categoryRepository = fixture.CreateCategoryRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        var expense =
            fixture.CreateExpense(status: Devlivery.Features.Expenses.Domain.Aggregates.Expenses.Enums.ExpenseStatus
                .Pending);
        expenseRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(expense);

        var newCategoryId = Guid.NewGuid();
        categoryRepository.GetByIdAsync(newCategoryId, Arg.Any<CancellationToken>())
            .Returns((Category?)null);

        var handler = new UpdateExpenseHandler(expenseRepository, categoryRepository, unitOfWork);

        var command = new UpdateExpenseCommand(
            ExpenseId: expense.Id,
            CategoryId: newCategoryId,
            Amount: null,
            DueDate: null,
            Supplier: null,
            Description: null
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_Should_Return_Fail_When_Category_Is_Inactive()
    {
        // Arrange
        var expenseRepository = fixture.CreateExpenseRepositoryMock();
        var categoryRepository = fixture.CreateCategoryRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        var expense =
            fixture.CreateExpense(status: Devlivery.Features.Expenses.Domain.Aggregates.Expenses.Enums.ExpenseStatus
                .Pending);
        expenseRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(expense);

        var inactiveCategory = fixture.CreateCategory(isActive: false);
        categoryRepository.GetByIdAsync(inactiveCategory.Id, Arg.Any<CancellationToken>())
            .Returns(inactiveCategory);

        var handler = new UpdateExpenseHandler(expenseRepository, categoryRepository, unitOfWork);

        var command = new UpdateExpenseCommand(
            ExpenseId: expense.Id,
            CategoryId: inactiveCategory.Id,
            Amount: null,
            DueDate: null,
            Supplier: null,
            Description: null
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_Should_Update_Expense_With_Correct_Properties()
    {
        // Arrange
        var expenseRepository = fixture.CreateExpenseRepositoryMock();
        var categoryRepository = fixture.CreateCategoryRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        var expense =
            fixture.CreateExpense(status: Devlivery.Features.Expenses.Domain.Aggregates.Expenses.Enums.ExpenseStatus
                .Pending);
        expenseRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(expense);

        var newCategory = fixture.CreateCategory(isActive: true);
        categoryRepository.GetByIdAsync(newCategory.Id, Arg.Any<CancellationToken>())
            .Returns(newCategory);

        var handler = new UpdateExpenseHandler(expenseRepository, categoryRepository, unitOfWork);

        const decimal newAmount = 250.00m;
        var newDueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14));
        const string newSupplier = "Novo Fornecedor";
        const string newDescription = "Nova Descrição";

        var command = new UpdateExpenseCommand(
            ExpenseId: expense.Id,
            CategoryId: newCategory.Id,
            Amount: newAmount,
            DueDate: newDueDate,
            Supplier: newSupplier,
            Description: newDescription
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        expense.Amount.ShouldBe(newAmount);
        expense.DueDate.ShouldBe(newDueDate);
        expense.Supplier.ShouldBe(newSupplier);
        expense.Description.ShouldBe(newDescription);
        expense.CategoryId.ShouldBe(newCategory.Id);

        await expenseRepository.Received(1).UpdateAsync(expense, Arg.Any<CancellationToken>());
    }
}