using Devlivery.Domain.Aggregates.CashRegister;
using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Domain.Common.Enums;
using Devlivery.Domain.SeedWork;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Domain;

[Trait("Category", "Unit Tests")]
public sealed class CashSessionTests(CashRegisterUnitTestFixture fixture) : IClassFixture<CashRegisterUnitTestFixture>
{
    [Fact]
    public void Constructor_Should_Create_CashSession_With_Correct_Properties()
    {
        // Arrange
        var establishmentId = Guid.NewGuid();
        var attendantId = Guid.NewGuid();
        const string attendantName = "João Silva";
        const decimal openingAmount = 100.00m;
        const string notes = "Abertura de caixa";

        // Act
        var cashSession = new CashSession(
            establishmentId,
            attendantId,
            attendantName,
            openingAmount,
            notes);

        // Assert
        cashSession.EstablishmentId.ShouldBe(establishmentId);
        cashSession.AttendantId.ShouldBe(attendantId);
        cashSession.AttendantName.ShouldBe(attendantName);
        cashSession.OpeningAmount.ShouldBe(openingAmount);
        cashSession.ExpectedCashAmount.ShouldBe(openingAmount);
        cashSession.Notes.ShouldBe(notes);
        cashSession.Status.ShouldBe(CashSessionStatus.Open);
        cashSession.TotalRevenue.ShouldBe(0m);
        cashSession.TotalOrders.ShouldBe(0);
        cashSession.ClosingAmount.ShouldBeNull();
        cashSession.EndAt.ShouldBeNull();
    }

    [Fact]
    public void AddPayment_Should_Increase_Revenue_And_Order_Count()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession(openingAmount: 100.00m);
        const decimal orderTotal = 50.00m;
        const PaymentMethod paymentMethod = PaymentMethod.Cash;

        // Act
        cashSession.AddPayment(Guid.NewGuid(), orderTotal, paymentMethod, Guid.NewGuid());

        // Assert
        cashSession.TotalRevenue.ShouldBe(50.00m);
        cashSession.TotalOrders.ShouldBe(1);
        cashSession.ExpectedCashAmount.ShouldBe(150.00m); // 100 + 50
    }

    [Fact]
    public void AddPayment_Should_Update_Payment_Breakdown()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession();

        // Act
        cashSession.AddPayment(Guid.NewGuid(), 25.00m, PaymentMethod.Cash, Guid.NewGuid());
        cashSession.AddPayment(Guid.NewGuid(), 30.00m, PaymentMethod.CreditCard, Guid.NewGuid());
        cashSession.AddPayment(Guid.NewGuid(), 15.00m, PaymentMethod.Cash, Guid.NewGuid());

        // Assert: totals and payments ledger reflect the operations
        cashSession.TotalRevenue.ShouldBe(70.00m);
        cashSession.Movements.Count.ShouldBe(3);
        cashSession.Movements.Count(p => p.PaymentMethod == PaymentMethod.Cash).ShouldBe(2);
        cashSession.Movements.Where(p => p.PaymentMethod == PaymentMethod.Cash).Sum(p => p.Amount).ShouldBe(40.00m);
        cashSession.Movements.Where(p => p.PaymentMethod == PaymentMethod.CreditCard).Sum(p => p.Amount)
            .ShouldBe(30.00m);
    }

    [Fact]
    public void AddPayment_With_Cash_Should_Update_Expected_Cash_Amount()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession(openingAmount: 100.00m);

        // Act
        cashSession.AddPayment(Guid.NewGuid(), 50.00m, PaymentMethod.Cash, Guid.NewGuid());
        cashSession.AddPayment(Guid.NewGuid(), 30.00m, PaymentMethod.Cash, Guid.NewGuid());

        // Assert
        cashSession.ExpectedCashAmount.ShouldBe(180.00m); // 100 + 50 + 30
    }

    [Fact]
    public void AddPayment_With_Card_Should_Not_Update_Expected_Cash_Amount()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession(openingAmount: 100.00m);

        // Act
        cashSession.AddPayment(Guid.NewGuid(), 50.00m, PaymentMethod.CreditCard, Guid.NewGuid());
        cashSession.AddPayment(Guid.NewGuid(), 30.00m, PaymentMethod.Pix, Guid.NewGuid());

        // Assert
        cashSession.ExpectedCashAmount.ShouldBe(100.00m); // Não muda
    }

    [Fact]
    public void AddPayment_Should_Be_Idempotent_For_Same_OrderPaymentId()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession(openingAmount: 100.00m);
        var paymentId = Guid.NewGuid();

        // Act
        cashSession.AddPayment(paymentId, 50.00m, PaymentMethod.Cash, Guid.NewGuid());
        cashSession.AddPayment(paymentId, 50.00m, PaymentMethod.Cash, Guid.NewGuid()); // Duplicate

        // Assert
        cashSession.TotalRevenue.ShouldBe(50.00m);
        cashSession.TotalOrders.ShouldBe(1);
        cashSession.Movements.Count.ShouldBe(1);
    }

    [Fact]
    public void AddDeposit_Should_Add_To_Deposits_Collection()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession(openingAmount: 100.00m);
        var attendantId = Guid.NewGuid();

        // Act
        cashSession.AddDeposit(50.00m, attendantId, "Depósito inicial");

        // Assert
        cashSession.Movements.Count(m => m.EntryType == CashSessionEntryType.Deposit).ShouldBe(1);
        cashSession.Movements.First(m => m.EntryType == CashSessionEntryType.Deposit).Amount.ShouldBe(50.00m);
    }

    [Fact]
    public void AddDeposit_Should_Update_Expected_Cash_Amount()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession(openingAmount: 100.00m);
        var attendantId = Guid.NewGuid();

        // Act
        cashSession.AddDeposit(50.00m, attendantId, "Depósito inicial");

        // Assert
        cashSession.ExpectedCashAmount.ShouldBe(150.00m); // 100 + 50
    }

    [Fact]
    public void AddPayment_NegativeAmount_Throws_DomainException()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession();

        // Act & Assert
        Should.Throw<DomainException>(() =>
            cashSession.AddPayment(Guid.NewGuid(), -10.00m, PaymentMethod.Cash, Guid.NewGuid()));
    }

    [Fact]
    public void AddReversal_Should_Add_Refund_And_Adjust_Revenue()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession(openingAmount: 100.00m);
        var orderPaymentId = Guid.NewGuid();
        var relatedOrderId = Guid.NewGuid();

        // A payment first
        cashSession.AddPayment(orderPaymentId, 50.00m, PaymentMethod.Cash, relatedOrderId);

        // Act: add reversal
        cashSession.AddReversal(orderPaymentId, 50.00m, PaymentMethod.Cash, "Customer refund", relatedOrderId);

        // Assert
        cashSession.TotalRevenue.ShouldBe(0.00m);
        cashSession.Movements.Count(m => m.EntryType == CashSessionEntryType.Refund).ShouldBe(1);
    }

    [Fact]
    public void AddReversal_Duplicate_Is_Idempotent()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession(openingAmount: 100.00m);
        var originalPaymentId = Guid.NewGuid();

        // Act
        cashSession.AddReversal(originalPaymentId, 30.00m, PaymentMethod.Cash, "reason", Guid.NewGuid());
        cashSession.AddReversal(originalPaymentId, 30.00m, PaymentMethod.Cash, "reason", Guid.NewGuid()); // duplicate

        // Assert
        cashSession.Movements.Count(m => m.EntryType == CashSessionEntryType.Refund).ShouldBe(1);
    }

    [Fact]
    public void AddChange_NonPositive_Does_Not_Add_Movement()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession();
        var relatedOrderId = Guid.NewGuid();
        var before = cashSession.Movements.Count;

        // Act
        cashSession.AddChange(relatedOrderId, 0m);
        cashSession.AddChange(relatedOrderId, -5m);

        // Assert
        cashSession.Movements.Count.ShouldBe(before);
    }

    [Fact]
    public void TotalCashPayments_Computes_Payments_Refunds_And_Change()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession();
        var order1 = Guid.NewGuid();
        var order2 = Guid.NewGuid();

        cashSession.AddPayment(Guid.NewGuid(), 40.00m, PaymentMethod.Cash, order1);
        cashSession.AddPayment(Guid.NewGuid(), 20.00m, PaymentMethod.Cash, order2);
        cashSession.AddPayment(Guid.NewGuid(), 30.00m, PaymentMethod.CreditCard, Guid.NewGuid()); // non-cash

        var paidId = Guid.NewGuid();
        cashSession.AddPayment(paidId, 10.00m, PaymentMethod.Cash, Guid.NewGuid());
        cashSession.AddReversal(paidId, 5.00m, PaymentMethod.Cash, "partial refund", Guid.NewGuid());

        cashSession.AddChange(order1, 2.00m);

        // Act
        var totalCashPayments = cashSession.TotalCashPayments();

        // Assert
        // payments: 40 + 20 + 10 = 70; refunds: 5; change: 2 => 70 - 5 - 2 = 63
        totalCashPayments.ShouldBe(63.00m);
    }

    [Fact]
    public void TotalDeposits_Should_Sum_All_Deposits()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession();
        var attendantId = Guid.NewGuid();

        cashSession.AddDeposit(50.00m, attendantId, "Depósito 1");
        cashSession.AddDeposit(30.00m, attendantId, "Depósito 2");
        cashSession.AddDeposit(20.00m, attendantId, "Depósito 3");

        // Act
        var total = cashSession.TotalDeposits();

        // Assert
        total.ShouldBe(100.00m);
    }

    [Fact]
    public void Close_Should_Set_Closed_Status_And_End_Time()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession();
        const decimal closingAmount = 250.00m;
        const string notes = "Fechamento normal";

        // Act
        cashSession.Close(closingAmount, notes);

        // Assert
        cashSession.Status.ShouldBe(CashSessionStatus.Closed);
        cashSession.ClosingAmount.ShouldBe(closingAmount);
        cashSession.Notes.ShouldBe(notes);
        cashSession.EndAt.ShouldNotBeNull();
    }

    [Fact]
    public void Close_Should_Keep_Original_Notes_When_New_Notes_Is_Empty()
    {
        // Arrange
        const string originalNotes = "Observação inicial";
        var cashSession = fixture.CreateCashSession(notes: originalNotes);

        // Act
        cashSession.Close(100.00m, null);

        // Assert
        cashSession.Notes.ShouldBe(originalNotes);
    }

    [Fact]
    public void Close_Should_Throw_When_Already_Closed()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession();
        cashSession.Close(100.00m, null);

        // Act & Assert
        Should.Throw<DomainException>(() => cashSession.Close(100.00m, null))
            .Message.ShouldContain("já está fechado");
    }
}