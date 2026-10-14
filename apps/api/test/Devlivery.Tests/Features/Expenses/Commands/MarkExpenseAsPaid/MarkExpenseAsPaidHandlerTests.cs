using Devlivery.Common.Errors;
using Devlivery.Domain.Aggregates.Expenses.Abstractions;
using Devlivery.Domain.Aggregates.Expenses.Enums;
using Devlivery.Domain.SeedWork;
using Devlivery.Features.Expenses.Commands.MarkExpenseAsPaid;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Tests.Common.Builders;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.MarkExpenseAsPaid;

[Trait("Category", "Unit Tests")]
public sealed class MarkExpenseAsPaidHandlerTests
{
    private static readonly DateOnly PaymentDate = new(2026, 5, 8);

    private readonly IExpenseRepository _expenseRepository = Substitute.For<IExpenseRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly MarkExpenseAsPaidHandler _handler;

    public MarkExpenseAsPaidHandlerTests()
    {
        _handler = new MarkExpenseAsPaidHandler(_expenseRepository, _unitOfWork);
    }

    [Fact]
    public async Task GivenNonexistentExpense_WhenMarkingAsPaid_ThenReturnsNotFoundError()
    {
        // Arrange
        _expenseRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();

        // Act
        var result = await _handler.HandleAsync(new MarkExpenseAsPaidCommand(Guid.NewGuid(), PaymentDate),
            CancellationToken.None);

        // Assert
        result.HasError<NotFoundError>().ShouldBeTrue();
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenPendingExpense_WhenMarkingAsPaid_ThenStoresPaymentDateAndCommits()
    {
        // Arrange
        var expense = new ExpenseBuilder().Build();
        _expenseRepository.GetByIdAsync(expense.Id, Arg.Any<CancellationToken>()).Returns(expense);

        // Act
        var result = await _handler.HandleAsync(new MarkExpenseAsPaidCommand(expense.Id, PaymentDate),
            CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        expense.Status.ShouldBe(ExpenseStatus.Paid);
        expense.PaymentDate.ShouldBe(PaymentDate);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenCancelledExpense_WhenMarkingAsPaid_ThenThrowsDomainExceptionWithoutCommitting()
    {
        // Arrange
        var expense = new ExpenseBuilder().AsCancelled().Build();
        _expenseRepository.GetByIdAsync(expense.Id, Arg.Any<CancellationToken>()).Returns(expense);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            _handler.HandleAsync(new MarkExpenseAsPaidCommand(expense.Id, PaymentDate), CancellationToken.None));

        // Assert
        exception.ShouldBeOfType<DomainException>();
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }
}