using Devlivery.Common.Errors;
using Devlivery.Domain.Aggregates.Expenses.Abstractions;
using Devlivery.Features.Expenses.Commands.UpdateExpense;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Tests.Common.Builders;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.UpdateExpense;

[Trait("Category", "Unit Tests")]
public sealed class UpdateExpenseHandlerTests
{
    private static readonly DateOnly DueDate = new(2026, 5, 10);

    private readonly IExpenseRepository _expenseRepository = Substitute.For<IExpenseRepository>();
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateExpenseHandler _handler;

    public UpdateExpenseHandlerTests()
    {
        _handler = new UpdateExpenseHandler(_expenseRepository, _categoryRepository, _unitOfWork);
    }

    [Fact]
    public async Task GivenNonexistentExpense_WhenUpdating_ThenReturnsNotFoundError()
    {
        // Arrange
        _expenseRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();

        // Act
        var result = await _handler.HandleAsync(
            new UpdateExpenseCommand(Guid.NewGuid(), null, 200m, null, null, null), CancellationToken.None);

        // Assert
        result.HasError<NotFoundError>().ShouldBeTrue();
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GivenMissingOrInactiveNewCategory_WhenUpdating_ThenReturnsNotFoundErrorKeepingExpense(
        bool categoryExists)
    {
        // Arrange
        var originalCategoryId = Guid.NewGuid();
        var expense = new ExpenseBuilder().WithCategoryId(originalCategoryId).WithAmount(150m).Build();
        _expenseRepository.GetByIdAsync(expense.Id, Arg.Any<CancellationToken>()).Returns(expense);
        var newCategoryId = Guid.NewGuid();
        if (categoryExists)
            _categoryRepository.GetByIdAsync(newCategoryId, Arg.Any<CancellationToken>())
                .Returns(new CategoryBuilder().AsInactive().Build());
        else
            _categoryRepository.GetByIdAsync(newCategoryId, Arg.Any<CancellationToken>()).ReturnsNull();

        // Act
        var result = await _handler.HandleAsync(
            new UpdateExpenseCommand(expense.Id, newCategoryId, 200m, null, null, null), CancellationToken.None);

        // Assert
        result.HasError<NotFoundError>().ShouldBeTrue();
        expense.CategoryId.ShouldBe(originalCategoryId);
        expense.Amount.ShouldBe(150m);
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenActiveNewCategory_WhenUpdating_ThenAppliesChangesAndCommits()
    {
        // Arrange
        var expense = new ExpenseBuilder().Build();
        var newCategory = new CategoryBuilder().Build();
        _expenseRepository.GetByIdAsync(expense.Id, Arg.Any<CancellationToken>()).Returns(expense);
        _categoryRepository.GetByIdAsync(newCategory.Id, Arg.Any<CancellationToken>()).Returns(newCategory);
        var command = new UpdateExpenseCommand(expense.Id, newCategory.Id, 300m, DueDate, "Atacadão", "Embalagens");

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        expense.CategoryId.ShouldBe(newCategory.Id);
        expense.Amount.ShouldBe(300m);
        expense.DueDate.ShouldBe(DueDate);
        expense.Supplier.ShouldBe("Atacadão");
        expense.Description.ShouldBe("Embalagens");
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenNoCategoryInCommand_WhenUpdating_ThenKeepsCategoryWithoutLookingItUp()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var expense = new ExpenseBuilder().WithCategoryId(categoryId).Build();
        _expenseRepository.GetByIdAsync(expense.Id, Arg.Any<CancellationToken>()).Returns(expense);

        // Act
        var result = await _handler.HandleAsync(
            new UpdateExpenseCommand(expense.Id, null, 300m, null, null, null), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        expense.CategoryId.ShouldBe(categoryId);
        expense.Amount.ShouldBe(300m);
    }
}