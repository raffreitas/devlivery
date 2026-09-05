using Devlivery.Domain.Aggregates.Expenses;
using Devlivery.Domain.Aggregates.Expenses.Enums;
using Devlivery.Features.Expenses.Commands.UpdateExpense;

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
            fixture.CreateExpense(status: ExpenseStatus
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
            fixture.CreateExpense(status: ExpenseStatus
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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Handle_Should_Update_Expense_With_Correct_Properties(bool isPaid)
    {
        // Arrange
        var expenseRepository = fixture.CreateExpenseRepositoryMock();
        var categoryRepository = fixture.CreateCategoryRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        DateOnly? paymentDate = isPaid ? DateOnly.FromDateTime(DateTime.UtcNow) : null;
        var expense = fixture.CreateExpense(paymentDate: paymentDate);
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
        DateOnly? correctedPaymentDate = isPaid ? paymentDate!.Value.AddDays(-1) : null;

        var command = new UpdateExpenseCommand(
            ExpenseId: expense.Id,
            CategoryId: newCategory.Id,
            Amount: newAmount,
            DueDate: newDueDate,
            Supplier: newSupplier,
            Description: newDescription,
            PaymentDate: correctedPaymentDate
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
        expense.Status.ShouldBe(isPaid ? ExpenseStatus.Paid : ExpenseStatus.Pending);
        expense.PaymentDate.ShouldBe(correctedPaymentDate);

        await expenseRepository.Received(1).UpdateAsync(expense, Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
