using Devlivery.Domain.Aggregates.Expenses;
using Devlivery.Domain.Aggregates.Expenses.Enums;
using Devlivery.Domain.SeedWork;
using Devlivery.Tests.Common.Builders;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Domain;

[Trait("Category", "Unit Tests")]
public sealed class ExpenseTests
{
    private static readonly DateOnly DueDate = new(2026, 5, 10);
    private static readonly DateOnly PaymentDate = new(2026, 5, 8);

    [Fact]
    public void GivenDataWithoutPaymentDate_WhenCreatingExpense_ThenStartsPending()
    {
        // Arrange
        var establishmentId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        // Act
        var expense = new Expense(establishmentId, categoryId, 150m, DueDate, "Distribuidora Sul", "Farinha");

        // Assert
        expense.Status.ShouldBe(ExpenseStatus.Pending);
        expense.PaymentDate.ShouldBeNull();
        expense.EstablishmentId.ShouldBe(establishmentId);
        expense.CategoryId.ShouldBe(categoryId);
        expense.Amount.ShouldBe(150m);
        expense.DueDate.ShouldBe(DueDate);
        expense.Supplier.ShouldBe("Distribuidora Sul");
        expense.Description.ShouldBe("Farinha");
    }

    [Fact]
    public void GivenPaymentDate_WhenCreatingExpense_ThenStartsPaid()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        // Act
        var expense = new Expense(Guid.NewGuid(), categoryId, 150m, DueDate, paymentDate: PaymentDate);

        // Assert
        expense.Status.ShouldBe(ExpenseStatus.Paid);
        expense.PaymentDate.ShouldBe(PaymentDate);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    public void GivenNonPositiveAmount_WhenCreatingExpense_ThenThrowsDomainException(decimal amount)
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        // Act
        var exception = Record.Exception(() => new Expense(Guid.NewGuid(), categoryId, amount, DueDate));

        // Assert
        exception.ShouldBeOfType<DomainException>().Message.ShouldBe("O valor da despesa deve ser maior que zero.");
    }

    [Fact]
    public void GivenPendingExpense_WhenUpdatingAllFields_ThenReplacesValues()
    {
        // Arrange
        var expense = new ExpenseBuilder().Build();
        var newCategoryId = Guid.NewGuid();
        var beforeUpdate = DateTime.UtcNow;

        // Act
        expense.Update(newCategoryId, 300m, DueDate, "Atacadão", "Embalagens");

        // Assert
        expense.CategoryId.ShouldBe(newCategoryId);
        expense.Amount.ShouldBe(300m);
        expense.DueDate.ShouldBe(DueDate);
        expense.Supplier.ShouldBe("Atacadão");
        expense.Description.ShouldBe("Embalagens");
        expense.UpdatedAt.ShouldBeGreaterThanOrEqualTo(beforeUpdate);
    }

    [Fact]
    public void GivenExpense_WhenUpdatingWithNullFields_ThenKeepsOriginalValues()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var expense = new ExpenseBuilder().WithCategoryId(categoryId).WithAmount(150m).WithDueDate(DueDate)
            .WithSupplier("Distribuidora Sul").WithDescription("Farinha").Build();

        // Act
        expense.Update();

        // Assert
        expense.CategoryId.ShouldBe(categoryId);
        expense.Amount.ShouldBe(150m);
        expense.DueDate.ShouldBe(DueDate);
        expense.Supplier.ShouldBe("Distribuidora Sul");
        expense.Description.ShouldBe("Farinha");
    }

    [Fact]
    public void GivenPaidExpense_WhenCorrectingPaymentDate_ThenUpdatesItAndStaysPaid()
    {
        // Arrange
        var expense = new ExpenseBuilder().AsPaid(PaymentDate).Build();

        // Act
        expense.Update(paymentDate: PaymentDate.AddDays(1), amount: 200m);

        // Assert
        expense.PaymentDate.ShouldBe(PaymentDate.AddDays(1));
        expense.Amount.ShouldBe(200m);
        expense.Status.ShouldBe(ExpenseStatus.Paid);
    }

    [Fact]
    public void GivenPendingExpense_WhenUpdatingPaymentDate_ThenThrowsDomainException()
    {
        // Arrange
        var expense = new ExpenseBuilder().Build();

        // Act
        var exception = Record.Exception(() => expense.Update(paymentDate: PaymentDate));

        // Assert
        exception.ShouldBeOfType<DomainException>()
            .Message.ShouldBe("Só é possível corrigir a data de pagamento de uma despesa paga.");
        expense.PaymentDate.ShouldBeNull();
    }

    [Fact]
    public void GivenPaidExpense_WhenUpdatingWithMinimumPaymentDate_ThenThrowsDomainException()
    {
        // Arrange
        var expense = new ExpenseBuilder().AsPaid(PaymentDate).Build();

        // Act
        var exception = Record.Exception(() => expense.Update(paymentDate: DateOnly.MinValue));

        // Assert
        exception.ShouldBeOfType<DomainException>().Message.ShouldBe("A data de pagamento deve ser válida.");
        expense.PaymentDate.ShouldBe(PaymentDate);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    public void GivenNonPositiveAmount_WhenUpdatingExpense_ThenThrowsDomainException(decimal amount)
    {
        // Arrange
        var expense = new ExpenseBuilder().WithAmount(150m).Build();

        // Act
        var exception = Record.Exception(() => expense.Update(amount: amount));

        // Assert
        exception.ShouldBeOfType<DomainException>().Message.ShouldBe("O valor da despesa deve ser maior que zero.");
        expense.Amount.ShouldBe(150m);
    }

    [Fact]
    public void GivenCancelledExpense_WhenUpdating_ThenThrowsDomainException()
    {
        // Arrange
        var expense = new ExpenseBuilder().WithAmount(150m).AsCancelled().Build();

        // Act
        var exception = Record.Exception(() => expense.Update(amount: 300m));

        // Assert
        exception.ShouldBeOfType<DomainException>();
        expense.Amount.ShouldBe(150m);
    }

    [Fact]
    public void GivenPendingExpense_WhenMarkingAsPaid_ThenStoresPaymentDate()
    {
        // Arrange
        var expense = new ExpenseBuilder().Build();

        // Act
        expense.MarkAsPaid(PaymentDate);

        // Assert
        expense.Status.ShouldBe(ExpenseStatus.Paid);
        expense.PaymentDate.ShouldBe(PaymentDate);
    }

    [Fact]
    public void GivenPaidExpense_WhenMarkingAsPaidAgain_ThenKeepsOriginalPaymentDate()
    {
        // Arrange
        var expense = new ExpenseBuilder().AsPaid(PaymentDate).Build();

        // Act
        expense.MarkAsPaid(PaymentDate.AddDays(5));

        // Assert
        expense.PaymentDate.ShouldBe(PaymentDate);
    }

    [Fact]
    public void GivenCancelledExpense_WhenMarkingAsPaid_ThenThrowsDomainException()
    {
        // Arrange
        var expense = new ExpenseBuilder().AsCancelled().Build();

        // Act
        var exception = Record.Exception(() => expense.MarkAsPaid(PaymentDate));

        // Assert
        exception.ShouldBeOfType<DomainException>().Message.ShouldBe("Não é possível pagar uma despesa cancelada.");
        expense.Status.ShouldBe(ExpenseStatus.Cancelled);
    }
}