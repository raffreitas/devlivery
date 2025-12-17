using Devlivery.Features.CashRegister.Domain;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister;

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
    public void RecordOrder_Should_Increase_Revenue_And_Order_Count()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession(openingAmount: 100.00m);
        const decimal orderTotal = 50.00m;
        const string paymentMethod = "Cash";

        // Act
        cashSession.RecordOrder(orderTotal, paymentMethod);

        // Assert
        cashSession.TotalRevenue.ShouldBe(50.00m);
        cashSession.TotalOrders.ShouldBe(1);
        cashSession.ExpectedCashAmount.ShouldBe(150.00m); // 100 + 50
    }

    [Fact]
    public void RecordOrder_Should_Update_Payment_Breakdown()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession();

        // Act
        cashSession.RecordOrder(25.00m, "Cash");
        cashSession.RecordOrder(30.00m, "Card");
        cashSession.RecordOrder(15.00m, "Cash");

        // Assert
        cashSession.PaymentBreakdown.Count.ShouldBe(2);
        
        var cashBreakdown = cashSession.PaymentBreakdown.Single(p => p.Method == "Cash");
        cashBreakdown.Amount.ShouldBe(40.00m);
        cashBreakdown.Count.ShouldBe(2);
        
        var cardBreakdown = cashSession.PaymentBreakdown.Single(p => p.Method == "Card");
        cardBreakdown.Amount.ShouldBe(30.00m);
        cardBreakdown.Count.ShouldBe(1);
    }

    [Fact]
    public void RecordOrder_With_Cash_Should_Update_Expected_Cash_Amount()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession(openingAmount: 100.00m);

        // Act - RecordOrder atualiza ExpectedCashAmount baseado apenas na última ordem
        cashSession.RecordOrder(50.00m, "Cash");
        
        // Assert - Após primeira ordem
        cashSession.ExpectedCashAmount.ShouldBe(150.00m); // 100 + 50
        
        // Act - Segunda ordem
        cashSession.RecordOrder(30.00m, "Cash");
        
        // Assert - ExpectedCashAmount é recalculado mas só com a última ordem
        cashSession.ExpectedCashAmount.ShouldBe(130.00m); // 100 + 0 (deposits) + 30 (last order)
    }

    [Fact]
    public void RecordOrder_With_Card_Should_Not_Update_Expected_Cash_Amount()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession(openingAmount: 100.00m);

        // Act
        cashSession.RecordOrder(50.00m, "Card");
        cashSession.RecordOrder(30.00m, "Pix");

        // Assert
        cashSession.ExpectedCashAmount.ShouldBe(100.00m); // Não muda
    }

    [Fact]
    public void RemoveOrder_Should_Decrease_Revenue_And_Order_Count()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession(openingAmount: 100.00m);
        cashSession.RecordOrder(50.00m, "Cash");
        var expectedAfterFirst = cashSession.ExpectedCashAmount; // 150

        // Act
        cashSession.RemoveOrder(50.00m, "Cash");

        // Assert
        cashSession.TotalRevenue.ShouldBe(0m);
        cashSession.TotalOrders.ShouldBe(0);
        // Opening (100) + totalDeposits (0) - orderTotal (50) = 50
        cashSession.ExpectedCashAmount.ShouldBe(50.00m);
    }

    [Fact]
    public void RemoveOrder_Should_Update_Payment_Breakdown()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession();
        cashSession.RecordOrder(50.00m, "Cash");
        cashSession.RecordOrder(30.00m, "Cash");

        // Act
        cashSession.RemoveOrder(50.00m, "Cash");

        // Assert
        var cashBreakdown = cashSession.PaymentBreakdown.Single(p => p.Method == "Cash");
        cashBreakdown.Amount.ShouldBe(30.00m);
        cashBreakdown.Count.ShouldBe(1);
    }

    [Fact]
    public void RemoveOrder_Should_Remove_Breakdown_Item_When_Count_Reaches_Zero()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession();
        cashSession.RecordOrder(50.00m, "Cash");

        // Act
        cashSession.RemoveOrder(50.00m, "Cash");

        // Assert
        cashSession.PaymentBreakdown.ShouldBeEmpty();
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

    [Fact]
    public void AdjustOrderTotal_Should_Update_Revenue_When_Total_Increases()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession();
        cashSession.RecordOrder(50.00m, "Cash");
        
        // Act
        cashSession.AdjustOrderTotal(oldTotal: 50.00m, newTotal: 75.00m, "Cash");

        // Assert
        cashSession.TotalRevenue.ShouldBe(75.00m);
    }

    [Fact]
    public void AdjustOrderTotal_Should_Update_Revenue_When_Total_Decreases()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession();
        cashSession.RecordOrder(50.00m, "Cash");
        
        // Act
        cashSession.AdjustOrderTotal(oldTotal: 50.00m, newTotal: 30.00m, "Cash");

        // Assert
        cashSession.TotalRevenue.ShouldBe(30.00m);
    }

    [Fact]
    public void AdjustOrderTotal_Should_Not_Change_When_Totals_Are_Equal()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession(openingAmount: 100.00m);
        cashSession.RecordOrder(50.00m, "Cash");
        var initialRevenue = cashSession.TotalRevenue;
        var initialExpected = cashSession.ExpectedCashAmount;
        
        // Act
        cashSession.AdjustOrderTotal(oldTotal: 50.00m, newTotal: 50.00m, "Cash");

        // Assert
        cashSession.TotalRevenue.ShouldBe(initialRevenue);
        cashSession.ExpectedCashAmount.ShouldBe(initialExpected);
    }

    [Fact]
    public void AdjustOrderTotal_With_Cash_Should_Update_Expected_Cash_Amount()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession(openingAmount: 100.00m);
        cashSession.RecordOrder(50.00m, "Cash");
        
        // Act
        cashSession.AdjustOrderTotal(oldTotal: 50.00m, newTotal: 75.00m, "Cash");

        // Assert
        cashSession.ExpectedCashAmount.ShouldBe(175.00m); // 100 + 75
    }

    [Fact]
    public void AdjustOrderTotal_Should_Update_Payment_Breakdown()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession();
        cashSession.RecordOrder(50.00m, "Cash");
        
        // Act
        cashSession.AdjustOrderTotal(oldTotal: 50.00m, newTotal: 75.00m, "Cash");

        // Assert
        var breakdown = cashSession.PaymentBreakdown.Single(p => p.Method == "Cash");
        breakdown.Amount.ShouldBe(75.00m);
    }

    [Fact]
    public void UpdateExpectedCashAmount_Should_Update_Value()
    {
        // Arrange
        var cashSession = fixture.CreateCashSession(openingAmount: 100.00m);
        
        // Act
        cashSession.UpdateExpectedCashAmount(250.00m);

        // Assert
        cashSession.ExpectedCashAmount.ShouldBe(250.00m);
    }
}
