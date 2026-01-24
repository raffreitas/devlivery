using Devlivery.Features.CashRegister.Domain.Enums;
using Devlivery.Shared.Domain.Enums;
using Devlivery.Shared.SeedWork;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Domain;

/// <summary>
/// Tests for concurrency and idempotency scenarios in CashSession aggregate.
/// These tests validate that concurrent operations don't lead to duplicate entries or data corruption.
/// </summary>
[Trait("Category", "Unit Tests")]
[Trait("Type", "Concurrency")]
public sealed class CashSessionConcurrencyTests(CashRegisterUnitTestFixture fixture)
    : IClassFixture<CashRegisterUnitTestFixture>
{
    [Fact(DisplayName = "AddPayment with same OrderPaymentId should be idempotent")]
    public void AddPayment_ConcurrentRequestsSamePaymentId_ShouldAddOnlyOnce()
    {
        // Arrange: Active cash session + same payment ID (simulates concurrent requests)
        var session = fixture.CreateCashSession(openingAmount: 100.00m);
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        // Act: Simulate 3 concurrent threads trying to add the same payment
        session.AddPayment(paymentId, 79.00m, PaymentMethod.Pix, orderId);
        session.AddPayment(paymentId, 79.00m, PaymentMethod.Pix, orderId); // Duplicate attempt 1
        session.AddPayment(paymentId, 79.00m, PaymentMethod.Pix, orderId); // Duplicate attempt 2

        // Assert: Only 1 movement created (idempotency protection)
        session.Movements.Count(m => m.OrderPaymentId == paymentId).ShouldBe(1);
        session.Movements.Count.ShouldBe(1);
        session.TotalRevenue.ShouldBe(79.00m);
        session.TotalOrders.ShouldBe(1);
    }

    [Fact(DisplayName = "AddPayment should handle multiple different payments correctly")]
    public void AddPayment_MultipleDifferentPayments_ShouldAddAll()
    {
        // Arrange
        var session = fixture.CreateCashSession(openingAmount: 100.00m);
        var payment1Id = Guid.NewGuid();
        var payment2Id = Guid.NewGuid();
        var payment3Id = Guid.NewGuid();

        // Act: Add 3 different payments
        session.AddPayment(payment1Id, 50.00m, PaymentMethod.Cash, Guid.NewGuid());
        session.AddPayment(payment2Id, 30.00m, PaymentMethod.Pix, Guid.NewGuid());
        session.AddPayment(payment3Id, 45.00m, PaymentMethod.CreditCard, Guid.NewGuid());

        // Assert: All 3 movements created
        session.Movements.Count.ShouldBe(3);
        session.TotalRevenue.ShouldBe(125.00m);
        session.TotalOrders.ShouldBe(3);
    }

    [Fact(DisplayName = "AddPayment should throw when session is closed")]
    public void AddPayment_WhenSessionClosed_ShouldThrowException()
    {
        // Arrange: Closed session
        var session = fixture.CreateCashSession(openingAmount: 100.00m);
        session.Close(150.00m, "Fechamento");

        // Act & Assert: Cannot add payments to closed session
        var exception = Should.Throw<DomainException>(() =>
            session.AddPayment(Guid.NewGuid(), 50.00m, PaymentMethod.Cash, Guid.NewGuid()));

        exception.Message.ShouldContain("fechado");
    }

    [Fact(DisplayName = "AddChange with same OrderId should be idempotent")]
    public void AddChange_ConcurrentRequestsSameOrderId_ShouldAddOnlyOnce()
    {
        // Arrange: Active session + same order ID
        var session = fixture.CreateCashSession(openingAmount: 100.00m);
        var orderId = Guid.NewGuid();

        // Act: Simulate concurrent requests trying to add change for same order
        session.AddChange(orderId, 30.00m);
        session.AddChange(orderId, 30.00m); // Duplicate attempt 1
        session.AddChange(orderId, 30.00m); // Duplicate attempt 2

        // Assert: Only 1 change entry created
        var changeMovements = session.Movements.Where(m =>
            m.EntryType == CashSessionEntryType.Change &&
            m.RelatedOrderId == orderId).ToList();

        changeMovements.Count.ShouldBe(1);
        changeMovements[0].Amount.ShouldBe(30.00m);
    }

    [Fact(DisplayName = "AddChange should not create entry when amount is zero")]
    public void AddChange_WithZeroAmount_ShouldNotAddEntry()
    {
        // Arrange
        var session = fixture.CreateCashSession(openingAmount: 100.00m);
        var orderId = Guid.NewGuid();

        // Act: Try to add zero change
        session.AddChange(orderId, 0.00m);

        // Assert: No change entry created
        session.Movements.Count(m => m.EntryType == CashSessionEntryType.Change).ShouldBe(0);
    }

    [Fact(DisplayName = "AddChange should not create entry when amount is negative")]
    public void AddChange_WithNegativeAmount_ShouldNotAddEntry()
    {
        // Arrange
        var session = fixture.CreateCashSession(openingAmount: 100.00m);
        var orderId = Guid.NewGuid();

        // Act: Try to add negative change
        session.AddChange(orderId, -10.00m);

        // Assert: No change entry created
        session.Movements.Count(m => m.EntryType == CashSessionEntryType.Change).ShouldBe(0);
    }

    [Fact(DisplayName = "AddChange should throw when session is closed")]
    public void AddChange_WhenSessionClosed_ShouldThrowException()
    {
        // Arrange: Closed session
        var session = fixture.CreateCashSession(openingAmount: 100.00m);
        session.Close(150.00m, "Fechamento");

        // Act & Assert
        Should.Throw<DomainException>(() =>
            session.AddChange(Guid.NewGuid(), 20.00m));
    }

    [Fact(DisplayName = "AddReversal should be idempotent for same payment")]
    public void AddReversal_ConcurrentRequestsSamePaymentId_ShouldAddOnlyOnce()
    {
        // Arrange: Session with an existing payment
        var session = fixture.CreateCashSession(openingAmount: 100.00m);
        var originalPaymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        session.AddPayment(originalPaymentId, 50.00m, PaymentMethod.Cash, orderId);

        // Act: Simulate concurrent reversal requests
        session.AddReversal(originalPaymentId, 50.00m, PaymentMethod.Cash, "Cancelamento", orderId);
        session.AddReversal(originalPaymentId, 50.00m, PaymentMethod.Cash, "Cancelamento", orderId); // Duplicate
        session.AddReversal(originalPaymentId, 50.00m, PaymentMethod.Cash, "Cancelamento", orderId); // Duplicate

        // Assert: Only 1 reversal created
        session.Movements.Count(m =>
            m.EntryType == CashSessionEntryType.Refund &&
            m.OrderPaymentId == originalPaymentId).ShouldBe(1);
    }

    [Fact(DisplayName = "AddReversal should throw when session is closed")]
    public void AddReversal_WhenSessionClosed_ShouldThrowException()
    {
        // Arrange
        var session = fixture.CreateCashSession(openingAmount: 100.00m);
        var paymentId = Guid.NewGuid();
        session.AddPayment(paymentId, 50.00m, PaymentMethod.Cash, Guid.NewGuid());
        session.Close(150.00m, "Fechamento");

        // Act & Assert
        Should.Throw<DomainException>(() =>
            session.AddReversal(paymentId, 50.00m, PaymentMethod.Cash, "Cancelamento", Guid.NewGuid()));
    }

    [Fact(DisplayName = "HasReversalFor should correctly detect existing reversals")]
    public void HasReversalFor_WhenReversalExists_ShouldReturnTrue()
    {
        // Arrange
        var session = fixture.CreateCashSession(openingAmount: 100.00m);
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        session.AddPayment(paymentId, 50.00m, PaymentMethod.Cash, orderId);
        session.AddReversal(paymentId, 50.00m, PaymentMethod.Cash, "Cancelamento", orderId);

        // Act & Assert
        session.HasReversalFor(paymentId).ShouldBeTrue();
    }

    [Fact(DisplayName = "HasReversalFor should return false when no reversal exists")]
    public void HasReversalFor_WhenNoReversal_ShouldReturnFalse()
    {
        // Arrange
        var session = fixture.CreateCashSession(openingAmount: 100.00m);
        var paymentId = Guid.NewGuid();

        // Act & Assert
        session.HasReversalFor(paymentId).ShouldBeFalse();
    }

    [Fact(DisplayName = "HasChangeFor should correctly detect existing change entries")]
    public void HasChangeFor_WhenChangeExists_ShouldReturnTrue()
    {
        // Arrange
        var session = fixture.CreateCashSession(openingAmount: 100.00m);
        var orderId = Guid.NewGuid();

        session.AddChange(orderId, 20.00m);

        // Act & Assert
        session.HasChangeFor(orderId).ShouldBeTrue();
    }

    [Fact(DisplayName = "HasChangeFor should return false when no change exists")]
    public void HasChangeFor_WhenNoChange_ShouldReturnFalse()
    {
        // Arrange
        var session = fixture.CreateCashSession(openingAmount: 100.00m);
        var orderId = Guid.NewGuid();

        // Act & Assert
        session.HasChangeFor(orderId).ShouldBeFalse();
    }

    [Fact(DisplayName = "Multiple operations should maintain correct totals")]
    public void MultipleOperations_ShouldMaintainCorrectTotals()
    {
        // Arrange
        var session = fixture.CreateCashSession(openingAmount: 100.00m);

        // Act: Complex scenario with payments, reversals, changes, and deposits
        var payment1Id = Guid.NewGuid();
        var payment2Id = Guid.NewGuid();
        var payment3Id = Guid.NewGuid();
        var order1Id = Guid.NewGuid();
        var order2Id = Guid.NewGuid();
        var order3Id = Guid.NewGuid();

        // Payment 1: Cash 80.00
        session.AddPayment(payment1Id, 80.00m, PaymentMethod.Cash, order1Id);
        session.AddChange(order1Id, 10.00m); // Change 10.00

        // Payment 2: Pix 50.00
        session.AddPayment(payment2Id, 50.00m, PaymentMethod.Pix, order2Id);

        // Payment 3: Cash 100.00, then reversed
        session.AddPayment(payment3Id, 100.00m, PaymentMethod.Cash, order3Id);
        session.AddReversal(payment3Id, 100.00m, PaymentMethod.Cash, "Cancelamento", order3Id);

        // Deposit: 50.00
        session.AddDeposit(50.00m, Guid.NewGuid(), "Reforço");

        // Assert: Calculate expected values
        // Total Revenue = 80 + 50 + 100 - 100 - 10 = 130.00
        session.TotalRevenue.ShouldBe(120.00m);

        // Total Orders = 3 (payment1, payment2, payment3 - even though payment3 was reversed)
        session.TotalOrders.ShouldBe(3);

        // Expected Cash = Opening + Deposits + Cash Payments - Refunds - Change
        // = 100 + 50 + (80 + 100) - 100 - 10 = 220.00
        session.ExpectedCashAmount.ShouldBe(220.00m);
    }

    [Fact(DisplayName = "Close should prevent multiple closings")]
    public void Close_WhenAlreadyClosed_ShouldThrowException()
    {
        // Arrange
        var session = fixture.CreateCashSession(openingAmount: 100.00m);
        session.Close(150.00m, "Primeiro fechamento");

        // Act & Assert: Cannot close twice
        var exception = Should.Throw<DomainException>(() =>
            session.Close(160.00m, "Tentativa de segundo fechamento"));

        exception.Message.ShouldContain("já está fechado");
    }

    [Fact(DisplayName = "Concurrent different payments should all be recorded")]
    public void AddPayment_ConcurrentDifferentPayments_ShouldRecordAll()
    {
        // Arrange: Simulate real-world scenario - 3 orders being delivered simultaneously
        var session = fixture.CreateCashSession(openingAmount: 100.00m);

        var order1 = (PaymentId: Guid.NewGuid(), OrderId: Guid.NewGuid(), Amount: 50.00m);
        var order2 = (PaymentId: Guid.NewGuid(), OrderId: Guid.NewGuid(), Amount: 75.00m);
        var order3 = (PaymentId: Guid.NewGuid(), OrderId: Guid.NewGuid(), Amount: 30.00m);

        // Act: Add all payments (simulates concurrent event handlers)
        session.AddPayment(order1.PaymentId, order1.Amount, PaymentMethod.Cash, order1.OrderId);
        session.AddPayment(order2.PaymentId, order2.Amount, PaymentMethod.Pix, order2.OrderId);
        session.AddPayment(order3.PaymentId, order3.Amount, PaymentMethod.CreditCard, order3.OrderId);

        // Assert: All payments recorded correctly
        session.Movements.Count.ShouldBe(3);
        session.TotalRevenue.ShouldBe(155.00m);
        session.TotalOrders.ShouldBe(3);

        // Verify each payment exists
        session.Movements.Any(m => m.OrderPaymentId == order1.PaymentId).ShouldBeTrue();
        session.Movements.Any(m => m.OrderPaymentId == order2.PaymentId).ShouldBeTrue();
        session.Movements.Any(m => m.OrderPaymentId == order3.PaymentId).ShouldBeTrue();
    }

    [Fact(DisplayName = "AddPayment should throw when amount is negative")]
    public void AddPayment_WithNegativeAmount_ShouldThrowException()
    {
        // Arrange
        var session = fixture.CreateCashSession(openingAmount: 100.00m);

        // Act & Assert
        Should.Throw<DomainException>(() =>
            session.AddPayment(Guid.NewGuid(), -50.00m, PaymentMethod.Cash, Guid.NewGuid()));
    }

    [Fact(DisplayName = "AddDeposit should throw when amount is negative")]
    public void AddDeposit_WithNegativeAmount_ShouldThrowException()
    {
        // Arrange
        var session = fixture.CreateCashSession(openingAmount: 100.00m);

        // Act & Assert
        Should.Throw<DomainException>(() =>
            session.AddDeposit(-50.00m, Guid.NewGuid(), "Tentativa inválida"));
    }

    [Fact(DisplayName = "AddReversal should throw when amount is negative")]
    public void AddReversal_WithNegativeAmount_ShouldThrowException()
    {
        // Arrange
        var session = fixture.CreateCashSession(openingAmount: 100.00m);
        var paymentId = Guid.NewGuid();

        // Act & Assert
        Should.Throw<DomainException>(() =>
            session.AddReversal(paymentId, -50.00m, PaymentMethod.Cash, "Tentativa inválida", Guid.NewGuid()));
    }
}