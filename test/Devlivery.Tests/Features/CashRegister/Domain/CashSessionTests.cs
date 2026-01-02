using Devlivery.Features.CashRegister.Domain;
using Devlivery.Features.CashRegister.Domain.Enums;
using Devlivery.Shared.Domain.Enums;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Domain;

[Collection("CashRegister Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class CashSessionTests(CashRegisterUnitTestFixture fixture)
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
        cashSession.Payments.Count.ShouldBe(3);
        cashSession.Payments.Count(p => p.PaymentMethod == PaymentMethod.Cash).ShouldBe(2);
        cashSession.Payments.Where(p => p.PaymentMethod == PaymentMethod.Cash).Sum(p => p.Amount).ShouldBe(40.00m);
        cashSession.Payments.Where(p => p.PaymentMethod == PaymentMethod.CreditCard).Sum(p => p.Amount)
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
        cashSession.Payments.Count.ShouldBe(1);
    }

    [Fact]
    public void AddDeposit_Should_Add_To_Deposits_Collection()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession(openingAmount: 100.00m);
        var deposit = fixture.CreateCashDeposit(
            cashSessionId: cashSession.Id,
            amount: 50.00m);

        // Act
        cashSession.AddDeposit(deposit);

        // Assert
        cashSession.Deposits.Count.ShouldBe(1);
        cashSession.Deposits.First().ShouldBe(deposit);
    }

    [Fact]
    public void AddDeposit_Should_Update_Expected_Cash_Amount()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession(openingAmount: 100.00m);
        var deposit = fixture.CreateCashDeposit(
            cashSessionId: cashSession.Id,
            amount: 50.00m);

        // Act
        cashSession.AddDeposit(deposit);

        // Assert
        cashSession.ExpectedCashAmount.ShouldBe(150.00m); // 100 + 50
    }

    [Fact]
    public void TotalDeposits_Should_Sum_All_Deposits()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession();
        var deposit1 = fixture.CreateCashDeposit(cashSessionId: cashSession.Id, amount: 50.00m);
        var deposit2 = fixture.CreateCashDeposit(cashSessionId: cashSession.Id, amount: 30.00m);
        var deposit3 = fixture.CreateCashDeposit(cashSessionId: cashSession.Id, amount: 20.00m);

        cashSession.AddDeposit(deposit1);
        cashSession.AddDeposit(deposit2);
        cashSession.AddDeposit(deposit3);

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
        Should.Throw<InvalidOperationException>(() => cashSession.Close(100.00m, null))
            .Message.ShouldContain("já está fechado");
    }
}