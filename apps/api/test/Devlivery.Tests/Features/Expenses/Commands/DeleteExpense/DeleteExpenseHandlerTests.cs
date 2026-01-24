using Devlivery.Features.Expenses.Commands.DeleteExpense;
using Devlivery.Features.Expenses.Domain.Aggregates.Expenses;
using Devlivery.Shared.Application.Errors;

using NSubstitute;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.DeleteExpense;

[Collection("Expenses Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class DeleteExpenseHandlerTests(ExpensesUnitTestFixture fixture)
{
    [Fact]
    public async Task Handle_Should_Return_NotFoundError_When_Expense_Does_Not_Exist()
    {
        // Arrange
        var expenseRepository = fixture.CreateExpenseRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        expenseRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Expense?)null);

        var handler = new DeleteExpenseHandler(expenseRepository, unitOfWork);

        var command = new DeleteExpenseCommand(ExpenseId: Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e is NotFoundError);
    }

    [Fact]
    public async Task Handle_Should_Delete_Expense_When_It_Exists()
    {
        // Arrange
        var expenseRepository = fixture.CreateExpenseRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        var expense = fixture.CreateExpense();
        expenseRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(expense);

        var handler = new DeleteExpenseHandler(expenseRepository, unitOfWork);

        var command = new DeleteExpenseCommand(ExpenseId: expense.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        await expenseRepository.Received(1).RemoveAsync(expense, Arg.Any<CancellationToken>());
    }
}