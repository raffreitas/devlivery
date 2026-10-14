using Devlivery.Common.Errors;
using Devlivery.Domain.Aggregates.Expenses;
using Devlivery.Domain.Aggregates.Expenses.Abstractions;
using Devlivery.Domain.Aggregates.Expenses.Enums;
using Devlivery.Features.Expenses.Commands.CreateExpense;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Tests.Common.Builders;
using Devlivery.Tests.Common.Mocks;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.CreateExpense;

[Trait("Category", "Unit Tests")]
public sealed class CreateExpenseHandlerTests
{
    private static readonly DateOnly DueDate = new(2026, 5, 10);

    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly IExpenseRepository _expenseRepository = Substitute.For<IExpenseRepository>();
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateExpenseHandler _handler;

    public CreateExpenseHandlerTests()
    {
        _handler = new CreateExpenseHandler(_expenseRepository, _categoryRepository, _unitOfWork,
            TestMocks.TenantAccessor(_tenantId));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GivenMissingOrInactiveCategory_WhenCreating_ThenReturnsNotFoundErrorWithoutPersisting(
        bool categoryExists)
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        if (categoryExists)
            _categoryRepository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
                .Returns(new CategoryBuilder().AsInactive().Build());
        else
            _categoryRepository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>()).ReturnsNull();

        // Act
        var result = await _handler.HandleAsync(new CreateExpenseCommand(categoryId, 150m, DueDate, null, null, null),
            CancellationToken.None);

        // Assert
        result.HasError<NotFoundError>().ShouldBeTrue();
        await _expenseRepository.DidNotReceive().AddAsync(Arg.Any<Expense>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenActiveCategory_WhenCreating_ThenPersistsPendingExpenseForCurrentTenant()
    {
        // Arrange
        var category = new CategoryBuilder().Build();
        _categoryRepository.GetByIdAsync(category.Id, Arg.Any<CancellationToken>()).Returns(category);
        Expense? addedExpense = null;
        await _expenseRepository.AddAsync(Arg.Do<Expense>(e => addedExpense = e), Arg.Any<CancellationToken>());
        var command = new CreateExpenseCommand(category.Id, 150m, DueDate, "Distribuidora Sul", "Farinha", null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        addedExpense.ShouldNotBeNull();
        addedExpense.EstablishmentId.ShouldBe(_tenantId);
        addedExpense.CategoryId.ShouldBe(category.Id);
        addedExpense.Amount.ShouldBe(150m);
        addedExpense.DueDate.ShouldBe(DueDate);
        addedExpense.Supplier.ShouldBe("Distribuidora Sul");
        addedExpense.Description.ShouldBe("Farinha");
        addedExpense.Status.ShouldBe(ExpenseStatus.Pending);
        result.Value.ExpenseId.ShouldBe(addedExpense.Id);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenPaymentDate_WhenCreating_ThenPersistsExpenseAlreadyPaid()
    {
        // Arrange
        var category = new CategoryBuilder().Build();
        _categoryRepository.GetByIdAsync(category.Id, Arg.Any<CancellationToken>()).Returns(category);
        Expense? addedExpense = null;
        await _expenseRepository.AddAsync(Arg.Do<Expense>(e => addedExpense = e), Arg.Any<CancellationToken>());
        var paymentDate = DueDate.AddDays(-2);

        // Act
        await _handler.HandleAsync(new CreateExpenseCommand(category.Id, 150m, DueDate, null, null, paymentDate),
            CancellationToken.None);

        // Assert
        addedExpense.ShouldNotBeNull();
        addedExpense.Status.ShouldBe(ExpenseStatus.Paid);
        addedExpense.PaymentDate.ShouldBe(paymentDate);
    }
}