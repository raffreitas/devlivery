using Devlivery.Domain.Aggregates.Expenses;
using Devlivery.Domain.Aggregates.Expenses.Enums;
using Devlivery.Domain.SeedWork;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Domain;

[Collection("Expenses Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class ExpenseTests(ExpensesUnitTestFixture fixture)
{
    [Fact]
    public void Constructor_Should_Create_Expense_With_Correct_Properties()
    {
        // Arrange
        var establishmentId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        const decimal amount = 150.50m;
        var dueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        const string supplier = "Fornecedor Teste";
        const string description = "Descrição da despesa";

        // Act
        var expense = new Expense(
            establishmentId: establishmentId,
            categoryId: categoryId,
            amount: amount,
            dueDate: dueDate,
            supplier: supplier,
            description: description
        );

        // Assert
        expense.EstablishmentId.ShouldBe(establishmentId);
        expense.CategoryId.ShouldBe(categoryId);
        expense.Amount.ShouldBe(amount);
        expense.DueDate.ShouldBe(dueDate);
        expense.Supplier.ShouldBe(supplier);
        expense.Description.ShouldBe(description);
        expense.Status.ShouldBe(ExpenseStatus.Pending);
        expense.PaymentDate.ShouldBeNull();
        expense.CreatedAt.ShouldNotBe(default);
        expense.UpdatedAt.ShouldNotBe(default);
    }

    [Fact]
    public void Constructor_Should_Set_Status_As_Paid_When_PaymentDate_Is_Provided()
    {
        // Arrange
        var paymentDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var expense = fixture.CreateExpense(paymentDate: paymentDate);

        // Assert
        expense.Status.ShouldBe(ExpenseStatus.Paid);
        expense.PaymentDate.ShouldBe(paymentDate);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Amount_Is_Zero()
    {
        // Act & Assert
        Should.Throw<DomainException>(() => new Expense(
            establishmentId: Guid.NewGuid(),
            categoryId: Guid.NewGuid(),
            amount: 0,
            dueDate: DateOnly.FromDateTime(DateTime.UtcNow)
        ));
    }

    [Fact]
    public void Constructor_Should_Throw_When_Amount_Is_Negative()
    {
        // Act & Assert
        Should.Throw<DomainException>(() => new Expense(
            establishmentId: Guid.NewGuid(),
            categoryId: Guid.NewGuid(),
            amount: -10,
            dueDate: DateOnly.FromDateTime(DateTime.UtcNow)
        ));
    }

    [Fact]
    public void IsOverdue_Should_Return_True_When_Expense_Is_Pending_And_DueDate_Is_Past()
    {
        // Arrange
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5));
        var expense = fixture.CreateExpense(dueDate: pastDate, status: ExpenseStatus.Pending);
        var referenceDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var result = expense.IsOverdue(referenceDate);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsOverdue_Should_Return_False_When_Expense_Is_Paid()
    {
        // Arrange
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5));
        var expense = fixture.CreateExpense(dueDate: pastDate, paymentDate: DateOnly.FromDateTime(DateTime.UtcNow));
        var referenceDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var result = expense.IsOverdue(referenceDate);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsOverdue_Should_Return_False_When_DueDate_Is_Future()
    {
        // Arrange
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));
        var expense = fixture.CreateExpense(dueDate: futureDate, status: ExpenseStatus.Pending);
        var referenceDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var result = expense.IsOverdue(referenceDate);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsDueToday_Should_Return_True_When_Expense_Is_Pending_And_DueDate_Is_Today()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expense = fixture.CreateExpense(dueDate: today, status: ExpenseStatus.Pending);

        // Act
        var result = expense.IsDueToday(today);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsDueToday_Should_Return_False_When_Expense_Is_Paid()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expense = fixture.CreateExpense(dueDate: today, paymentDate: today);

        // Act
        var result = expense.IsDueToday(today);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task Update_Should_Update_All_Properties()
    {
        // Arrange
        var expense = fixture.CreateExpense(
            amount: 100.00m,
            dueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            supplier: "Fornecedor Original",
            description: "Descrição Original"
        );

        var newCategoryId = Guid.NewGuid();
        const decimal newAmount = 200.00m;
        var newDueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14));
        const string newSupplier = "Novo Fornecedor";
        const string newDescription = "Nova Descrição";
        var originalUpdatedAt = expense.UpdatedAt;
        await Task.Delay(10);

        // Act
        expense.Update(
            categoryId: newCategoryId,
            amount: newAmount,
            dueDate: newDueDate,
            supplier: newSupplier,
            description: newDescription
        );

        // Assert
        expense.CategoryId.ShouldBe(newCategoryId);
        expense.Amount.ShouldBe(newAmount);
        expense.DueDate.ShouldBe(newDueDate);
        expense.Supplier.ShouldBe(newSupplier);
        expense.Description.ShouldBe(newDescription);
        expense.UpdatedAt.ShouldBeGreaterThan(originalUpdatedAt);
    }

    [Fact]
    public void Update_Should_Keep_Original_Values_When_Null()
    {
        // Arrange
        const decimal originalAmount = 100.00m;
        var originalDueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        const string originalSupplier = "Fornecedor Original";
        const string originalDescription = "Descrição Original";
        var originalCategoryId = Guid.NewGuid();

        var expense = fixture.CreateExpense(
            categoryId: originalCategoryId,
            amount: originalAmount,
            dueDate: originalDueDate,
            supplier: originalSupplier,
            description: originalDescription
        );

        // Act
        expense.Update(
            categoryId: null,
            amount: null,
            dueDate: null,
            supplier: null,
            description: null
        );

        // Assert
        expense.CategoryId.ShouldBe(originalCategoryId);
        expense.Amount.ShouldBe(originalAmount);
        expense.DueDate.ShouldBe(originalDueDate);
        expense.Supplier.ShouldBe(originalSupplier);
        expense.Description.ShouldBe(originalDescription);
    }

    [Fact]
    public async Task Update_Should_Update_UpdatedAt_Timestamp()
    {
        // Arrange
        var expense = fixture.CreateExpense();
        var originalUpdatedAt = expense.UpdatedAt;
        await Task.Delay(10);

        // Act
        expense.Update(categoryId: null, amount: 200.00m);

        // Assert
        expense.UpdatedAt.ShouldBeGreaterThan(originalUpdatedAt);
    }

    [Fact]
    public void Update_Should_Update_When_Expense_Is_Paid()
    {
        // Arrange
        var paymentDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var expense = fixture.CreateExpense(amount: 100.00m, paymentDate: paymentDate);

        // Act
        expense.Update(categoryId: null, amount: 200.00m);

        // Assert
        expense.Amount.ShouldBe(200.00m);
        expense.Status.ShouldBe(ExpenseStatus.Paid);
        expense.PaymentDate.ShouldBe(paymentDate);
    }

    [Fact]
    public void Update_Should_Correct_PaymentDate_When_Paid()
    {
        // Arrange
        var expense = fixture.CreateExpense(paymentDate: new DateOnly(2026, 9, 1));
        var correctedDate = new DateOnly(2026, 9, 2);

        // Act
        expense.Update(paymentDate: correctedDate);

        // Assert
        expense.PaymentDate.ShouldBe(correctedDate);
        expense.Status.ShouldBe(ExpenseStatus.Paid);
    }

    [Theory]
    [InlineData(ExpenseStatus.Pending)]
    [InlineData(ExpenseStatus.Cancelled)]
    public void Update_Should_Reject_PaymentDate_When_Not_Paid(ExpenseStatus status)
    {
        // Arrange
        var expense = fixture.CreateExpense(amount: 100m, status: status);

        // Act
        Should.Throw<DomainException>(() =>
            expense.Update(amount: 200m, paymentDate: new DateOnly(2026, 9, 2)));

        // Assert
        expense.Amount.ShouldBe(100m);
        expense.PaymentDate.ShouldBeNull();
        expense.Status.ShouldBe(status);
    }

    [Fact]
    public void Update_Should_Reject_Default_PaymentDate()
    {
        // Arrange
        var originalDate = new DateOnly(2026, 9, 1);
        var expense = fixture.CreateExpense(paymentDate: originalDate);

        // Act
        Should.Throw<DomainException>(() => expense.Update(paymentDate: DateOnly.MinValue));

        // Assert
        expense.PaymentDate.ShouldBe(originalDate);
    }

    [Fact]
    public void Update_Should_Throw_When_Expense_Is_Cancelled()
    {
        // Arrange
        var expense = fixture.CreateExpense(status: ExpenseStatus.Cancelled);

        // Act & Assert
        Should.Throw<DomainException>(() => expense.Update(amount: 200.00m));
    }

    [Fact]
    public void MarkAsPaid_Should_Set_Status_To_Paid()
    {
        // Arrange
        var expense = fixture.CreateExpense(status: ExpenseStatus.Pending);
        var paymentDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        expense.MarkAsPaid(paymentDate);

        // Assert
        expense.Status.ShouldBe(ExpenseStatus.Paid);
        expense.PaymentDate.ShouldBe(paymentDate);
    }

    [Fact]
    public async Task MarkAsPaid_Should_Update_UpdatedAt_Timestamp()
    {
        // Arrange
        var expense = fixture.CreateExpense(status: ExpenseStatus.Pending);
        var originalUpdatedAt = expense.UpdatedAt;
        await Task.Delay(10);
        var paymentDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        expense.MarkAsPaid(paymentDate);

        // Assert
        expense.UpdatedAt.ShouldBeGreaterThan(originalUpdatedAt);
    }

    [Fact]
    public void MarkAsPaid_Should_Do_Nothing_When_Already_Paid()
    {
        // Arrange
        var originalPaymentDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5));
        var expense = fixture.CreateExpense(paymentDate: originalPaymentDate);
        var newPaymentDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        expense.MarkAsPaid(newPaymentDate);

        // Assert
        expense.Status.ShouldBe(ExpenseStatus.Paid);
        expense.PaymentDate.ShouldBe(originalPaymentDate); // Não muda
    }

    [Fact]
    public void MarkAsPaid_Should_Throw_When_Expense_Is_Cancelled()
    {
        // Arrange
        var expense = fixture.CreateExpense(status: ExpenseStatus.Cancelled);
        var paymentDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act & Assert
        Should.Throw<DomainException>(() => expense.MarkAsPaid(paymentDate));
    }

    [Fact]
    public void Cancel_Should_Set_Status_To_Cancelled()
    {
        // Arrange
        var expense = fixture.CreateExpense(status: ExpenseStatus.Pending);

        // Act
        expense.Cancel();

        // Assert
        expense.Status.ShouldBe(ExpenseStatus.Cancelled);
    }

    [Fact]
    public async Task Cancel_Should_Update_UpdatedAt_Timestamp()
    {
        // Arrange
        var expense = fixture.CreateExpense(status: ExpenseStatus.Pending);
        var originalUpdatedAt = expense.UpdatedAt;
        await Task.Delay(10);

        // Act
        expense.Cancel();

        // Assert
        expense.UpdatedAt.ShouldBeGreaterThan(originalUpdatedAt);
    }

    [Fact]
    public void Cancel_Should_Throw_When_Expense_Is_Paid()
    {
        // Arrange
        var expense = fixture.CreateExpense(paymentDate: DateOnly.FromDateTime(DateTime.UtcNow));

        // Act & Assert
        Should.Throw<DomainException>(() => expense.Cancel());
    }

    [Fact]
    public void UnmarkAsPaid_Should_Set_Status_To_Pending()
    {
        // Arrange
        var expense = fixture.CreateExpense(paymentDate: DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        expense.UnmarkAsPaid();

        // Assert
        expense.Status.ShouldBe(ExpenseStatus.Pending);
        expense.PaymentDate.ShouldBeNull();
    }

    [Fact]
    public async Task UnmarkAsPaid_Should_Update_UpdatedAt_Timestamp()
    {
        // Arrange
        var expense = fixture.CreateExpense(paymentDate: DateOnly.FromDateTime(DateTime.UtcNow));
        var originalUpdatedAt = expense.UpdatedAt;
        await Task.Delay(10);

        // Act
        expense.UnmarkAsPaid();

        // Assert
        expense.UpdatedAt.ShouldBeGreaterThan(originalUpdatedAt);
    }

    [Fact]
    public void UnmarkAsPaid_Should_Do_Nothing_When_Not_Paid()
    {
        // Arrange
        var expense = fixture.CreateExpense(status: ExpenseStatus.Pending);

        // Act
        expense.UnmarkAsPaid();

        // Assert
        expense.Status.ShouldBe(ExpenseStatus.Pending);
        expense.PaymentDate.ShouldBeNull();
    }
}
