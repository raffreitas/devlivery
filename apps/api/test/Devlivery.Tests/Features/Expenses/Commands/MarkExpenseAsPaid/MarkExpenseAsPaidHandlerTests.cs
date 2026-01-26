using Devlivery.Domain.Aggregates.Expenses;
using Devlivery.Domain.Aggregates.Expenses.Enums;
using Devlivery.Features.Expenses.Commands.MarkExpenseAsPaid;

using NSubstitute;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.MarkExpenseAsPaid;

[Collection("Expenses Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class MarkExpenseAsPaidHandlerTests(ExpensesUnitTestFixture fixture)
{
    [Fact]
    public async Task Handle_Should_Return_Fail_When_Expense_Does_Not_Exist()
    {
        // Arrange
        var expenseRepository = fixture.CreateExpenseRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        expenseRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Expense?)null);

        var handler = new MarkExpenseAsPaidHandler(expenseRepository, unitOfWork);

        var command = new MarkExpenseAsPaidCommand(
            ExpenseId: Guid.NewGuid(),
            PaymentDate: DateOnly.FromDateTime(DateTime.UtcNow)
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_Should_Mark_Expense_As_Paid_When_It_Exists()
    {
        // Arrange
        var expenseRepository = fixture.CreateExpenseRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        var expense =
            fixture.CreateExpense(status: ExpenseStatus
                .Pending);
        expenseRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(expense);

        var handler = new MarkExpenseAsPaidHandler(expenseRepository, unitOfWork);

        var paymentDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var command = new MarkExpenseAsPaidCommand(
            ExpenseId: expense.Id,
            PaymentDate: paymentDate
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        expense.Status.ShouldBe(ExpenseStatus.Paid);
        expense.PaymentDate.ShouldBe(paymentDate);

        await expenseRepository.Received(1).UpdateAsync(expense, Arg.Any<CancellationToken>());
    }
}