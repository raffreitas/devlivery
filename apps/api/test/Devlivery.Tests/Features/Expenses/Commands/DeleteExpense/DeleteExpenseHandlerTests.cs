using Devlivery.Common.Errors;
using Devlivery.Domain.Aggregates.Expenses.Abstractions;
using Devlivery.Features.Expenses.Commands.DeleteExpense;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Tests.Common.Builders;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.DeleteExpense;

[Trait("Category", "Unit Tests")]
public sealed class DeleteExpenseHandlerTests
{
    private readonly IExpenseRepository _expenseRepository = Substitute.For<IExpenseRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteExpenseHandler _handler;

    public DeleteExpenseHandlerTests()
    {
        _handler = new DeleteExpenseHandler(_expenseRepository, _unitOfWork);
    }

    [Fact]
    public async Task GivenNonexistentExpense_WhenDeleting_ThenReturnsNotFoundError()
    {
        // Arrange
        _expenseRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();

        // Act
        var result = await _handler.HandleAsync(new DeleteExpenseCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.HasError<NotFoundError>().ShouldBeTrue();
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenExistingExpense_WhenDeleting_ThenRemovesItAndCommits()
    {
        // Arrange
        var expense = new ExpenseBuilder().Build();
        _expenseRepository.GetByIdAsync(expense.Id, Arg.Any<CancellationToken>()).Returns(expense);

        // Act
        var result = await _handler.HandleAsync(new DeleteExpenseCommand(expense.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _expenseRepository.Received(1).RemoveAsync(expense, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}