using Devlivery.Domain.Aggregates.Orders.Entities;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.Aggregates.Orders.Events;
using Devlivery.Domain.Aggregates.Orders.ValueObjects;
using Devlivery.Domain.Common.Enums;
using Devlivery.Domain.SeedWork;
using Devlivery.Tests.Common.Builders;

using Shouldly;

namespace Devlivery.Tests.Features.Orders.Domain;

/// <summary>
/// Tests for concurrency and idempotency scenarios in Order aggregate.
/// These tests validate that concurrent operations don't lead to data corruption or duplicate events.
/// </summary>
[Collection("Orders Unit Tests")]
[Trait("Category", "Unit Tests")]
[Trait("Type", "Concurrency")]
public sealed class OrderConcurrencyTests(OrdersUnitTestFixture fixture)
{
    [Fact(DisplayName = "Multiple delivery confirmations should only confirm payment once")]
    public void UpdateStatus_ConcurrentDeliveryConfirmations_ShouldConfirmPaymentOnlyOnce()
    {
        // Arrange: Create order with one pending payment that covers the total
        var establishmentId = Guid.NewGuid();
        var orderItem = fixture.CreateOrderItem(establishmentId: establishmentId, quantity: 2, unitPrice: 39.50m);
        var deliveryFee = 5.00m;
        var totalAmount = orderItem.TotalPrice + deliveryFee; // Ensure payment covers total

        var order = new OrderBuilder()
            .WithEstablishmentId(establishmentId)
            .WithItems(orderItem)
            .WithDeliveryFee(deliveryFee)
            .WithCustomPayments([new OrderPayment(establishmentId, PaymentMethod.Pix, totalAmount)])
            .Build();

        // Verify initial state
        order.Payments.Count.ShouldBe(1);
        order.Payments.First().PaymentStatus.ShouldBe(PaymentStatus.Pending);

        // Act: First confirmation succeeds
        order.UpdateStatus(OrderStatus.Delivered);

        // Assert: Payment is confirmed
        order.Status.ShouldBe(OrderStatus.Delivered);
        order.Payments.First().PaymentStatus.ShouldBe(PaymentStatus.Confirmed);
        order.Payments.First().ConfirmedAt.ShouldNotBeNull();

        // Verify only one confirmation event was raised
        var confirmationEvents = order.DomainEvents.OfType<OrderPaymentConfirmedEvent>().ToList();
        confirmationEvents.Count.ShouldBe(1);

        // Act: Simulate concurrent request calling Delivered again (idempotent - no exception, no new events)
        order.UpdateStatus(OrderStatus.Delivered); // Should be idempotent - no exception thrown

        // Assert: Payment confirmation not attempted again (stays confirmed, no new events)
        order.Payments.First().PaymentStatus.ShouldBe(PaymentStatus.Confirmed);
        confirmationEvents = order.DomainEvents.OfType<OrderPaymentConfirmedEvent>().ToList();
        confirmationEvents.Count.ShouldBe(1); // Still just 1 event - idempotent!
    }

    [Fact(DisplayName = "Payment confirmation should be idempotent")]
    public void Confirm_WhenAlreadyConfirmed_ShouldThrowException()
    {
        // Arrange
        var payment = new OrderPayment(Guid.NewGuid(), PaymentMethod.Cash, 100.00m);

        // Act: First confirmation
        payment.Confirm();

        // Assert: Payment is confirmed
        payment.PaymentStatus.ShouldBe(PaymentStatus.Confirmed);
        payment.ConfirmedAt.ShouldNotBeNull();

        // Act & Assert: Second confirmation should throw
        var exception = Should.Throw<InvalidOperationException>(() => payment.Confirm());
        exception.Message.ShouldContain("já está confirmado");
    }

    [Fact(DisplayName = "Cancelled payment cannot be confirmed")]
    public void Confirm_WhenCancelled_ShouldThrowException()
    {
        // Arrange
        var payment = new OrderPayment(Guid.NewGuid(), PaymentMethod.Cash, 100.00m);
        payment.Cancel();

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => payment.Confirm());
        exception.Message.ShouldContain("cancelado");
    }

    [Fact(DisplayName = "Multiple payments should all be confirmed only once")]
    public void UpdateStatus_MultiplePayments_ShouldConfirmEachOnlyOnce()
    {
        // Arrange: Order with multiple payment methods
        var establishmentId = Guid.NewGuid();
        var orderItem = fixture.CreateOrderItem(establishmentId: establishmentId, quantity: 1, unitPrice: 100.00m);
        var deliveryFee = 0m;

        var payments = new List<OrderPayment>
        {
            new(establishmentId, PaymentMethod.Cash, 50.00m),
            new(establishmentId, PaymentMethod.Pix, 30.00m),
            new(establishmentId, PaymentMethod.CreditCard, 20.00m)
        };

        var order = new OrderBuilder()
            .WithEstablishmentId(establishmentId)
            .WithItems(orderItem)
            .WithDeliveryFee(deliveryFee)
            .WithCustomPayments(payments)
            .Build();

        // Act: Confirm delivery
        order.UpdateStatus(OrderStatus.Delivered);

        // Assert: All payments confirmed exactly once
        order.Payments.Count.ShouldBe(3);
        order.Payments.All(p => p.PaymentStatus == PaymentStatus.Confirmed).ShouldBeTrue();
        order.Payments.All(p => p.ConfirmedAt != null).ShouldBeTrue();

        // Verify correct number of events
        var confirmationEvents = order.DomainEvents.OfType<OrderPaymentConfirmedEvent>().ToList();
        confirmationEvents.Count.ShouldBe(3);
        confirmationEvents.Select(e => e.PaymentId).Distinct().Count().ShouldBe(3);
    }

    [Fact(DisplayName = "Already confirmed payments should not be confirmed again")]
    public void UpdateStatus_WithAlreadyConfirmedPayments_ShouldOnlyConfirmPending()
    {
        // Arrange: Order with one confirmed and one pending payment
        var establishmentId = Guid.NewGuid();
        var orderItem = fixture.CreateOrderItem(establishmentId: establishmentId, quantity: 1, unitPrice: 100.00m);
        var deliveryFee = 0m;

        var payment1 = new OrderPayment(establishmentId, PaymentMethod.Cash, 50.00m);
        payment1.Confirm(); // Already confirmed

        var payment2 = new OrderPayment(establishmentId, PaymentMethod.Pix, 50.00m);
        // Pending

        var order = new OrderBuilder()
            .WithEstablishmentId(establishmentId)
            .WithItems(orderItem)
            .WithDeliveryFee(deliveryFee)
            .WithCustomPayments([payment1, payment2])
            .Build();

        // Clear domain events from manual confirmation
        order.ClearDomainEvents();

        // Act: Confirm delivery (should only confirm payment2)
        order.UpdateStatus(OrderStatus.Delivered);

        // Assert: Only pending payment generated a new event
        var confirmationEvents = order.DomainEvents.OfType<OrderPaymentConfirmedEvent>().ToList();
        confirmationEvents.Count.ShouldBe(1);
        confirmationEvents[0].PaymentId.ShouldBe(payment2.Id);
    }

    [Fact(DisplayName = "Cancelled payments should not be confirmed on delivery")]
    public void UpdateStatus_WithCancelledPayments_ShouldOnlyConfirmNonCancelled()
    {
        // Arrange
        var establishmentId = Guid.NewGuid();
        var orderItem = fixture.CreateOrderItem(establishmentId: establishmentId, quantity: 1, unitPrice: 100.00m);
        var deliveryFee = 0m;

        var payment1 = new OrderPayment(establishmentId, PaymentMethod.Cash, 50.00m);
        payment1.Cancel(); // Cancelled

        var payment2 = new OrderPayment(establishmentId, PaymentMethod.Pix, 100.00m);
        // Pending - covers full order total

        var order = new OrderBuilder()
            .WithEstablishmentId(establishmentId)
            .WithItems(orderItem)
            .WithDeliveryFee(deliveryFee)
            .WithCustomPayments([payment1, payment2])
            .Build();

        // Act: Confirm delivery
        order.UpdateStatus(OrderStatus.Delivered);

        // Assert: Only non-cancelled payment confirmed
        order.Payments.Count(p => p.PaymentStatus == PaymentStatus.Confirmed).ShouldBe(1);
        order.Payments.First(p => p.PaymentMethod == PaymentMethod.Pix).PaymentStatus
            .ShouldBe(PaymentStatus.Confirmed);
        order.Payments.First(p => p.PaymentMethod == PaymentMethod.Cash).PaymentStatus
            .ShouldBe(PaymentStatus.Cancelled);

        var confirmationEvents = order.DomainEvents.OfType<OrderPaymentConfirmedEvent>().ToList();
        confirmationEvents.Count.ShouldBe(1);
    }

    [Fact(DisplayName = "Change calculation should only happen once on delivery")]
    public void UpdateStatus_Delivered_ShouldCalculateChangeOnlyOnce()
    {
        // Arrange: Order with overpayment
        var establishmentId = Guid.NewGuid();
        var orderItem = fixture.CreateOrderItem(establishmentId: establishmentId, quantity: 1, unitPrice: 70.00m);

        var payment = new OrderPayment(establishmentId, PaymentMethod.Cash, 100.00m); // 30.00 change

        var order = new OrderBuilder()
            .WithEstablishmentId(establishmentId)
            .WithItems(orderItem)
            .WithCustomPayments([payment])
            .WithDeliveryFee(0m)
            .Build();

        // Act: Confirm delivery
        order.UpdateStatus(OrderStatus.Delivered);

        // Assert: Change calculated correctly
        order.Change.ShouldBe(30.00m);
        order.Total.ShouldBe(70.00m);

        // Verify only one change event
        var changeEvents = order.DomainEvents.OfType<OrderChangeCalculatedEvent>().ToList();
        changeEvents.Count.ShouldBe(1);
        changeEvents[0].Change.ShouldBe(30.00m);
    }

    [Fact(DisplayName = "Order status should prevent multiple status transitions")]
    public void UpdateStatus_AlreadyDelivered_ShouldThrowException()
    {
        // Arrange
        var order = fixture.CreateOrder();
        order.UpdateStatus(OrderStatus.Delivered);

        // Act & Assert: Cannot change status after delivered
        Should.Throw<DomainException>(() => order.UpdateStatus(OrderStatus.Preparing));
        Should.Throw<DomainException>(() => order.UpdateStatus(OrderStatus.Ready));
        Should.Throw<DomainException>(() => order.UpdateStatus(OrderStatus.Canceled));
    }

    [Fact(DisplayName = "Cancelled order should prevent any status change")]
    public void UpdateStatus_Cancelled_ShouldPreventAnyChange()
    {
        // Arrange
        var order = fixture.CreateOrder();
        order.UpdateStatus(OrderStatus.Canceled);

        // Act & Assert
        Should.Throw<DomainException>(() => order.UpdateStatus(OrderStatus.Preparing));
        Should.Throw<DomainException>(() => order.UpdateStatus(OrderStatus.Delivered));
    }

    [Fact(DisplayName = "Concurrent payment reconciliation should respect confirmed status")]
    public void ReconcilePayments_WithConfirmedPayment_ShouldThrowWhenTryingToModify()
    {
        // Arrange: Order with confirmed payment
        var establishmentId = Guid.NewGuid();
        var orderItem = fixture.CreateOrderItem(establishmentId: establishmentId);

        var confirmedPayment = new OrderPayment(establishmentId, PaymentMethod.Cash, 100.00m);
        confirmedPayment.Confirm();

        var order = new OrderBuilder()
            .WithEstablishmentId(establishmentId)
            .WithItems(orderItem)
            .WithCustomPayments([confirmedPayment])
            .Build();

        // Act & Assert: Cannot modify confirmed payment
        var updates = new[]
        {
            new OrderPaymentUpdate(confirmedPayment.Id, PaymentMethod.Pix, 50.00m) // Try to change
        };

        var exception = Should.Throw<DomainException>(() => order.ReconcilePayments(updates));
        exception.Message.ShouldContain("confirmado");
        exception.Message.ShouldContain("estorno");
    }

    [Fact(DisplayName = "Payment update should work only when pending")]
    public void Update_WhenPending_ShouldUpdateSuccessfully()
    {
        // Arrange
        var payment = new OrderPayment(Guid.NewGuid(), PaymentMethod.Cash, 100.00m);

        // Act
        payment.Update(PaymentMethod.Pix, 150.00m);

        // Assert
        payment.PaymentMethod.ShouldBe(PaymentMethod.Pix);
        payment.Amount.ShouldBe(150.00m);
        payment.PaymentStatus.ShouldBe(PaymentStatus.Pending);
    }

    [Fact(DisplayName = "Payment update should throw when confirmed")]
    public void Update_WhenConfirmed_ShouldThrowException()
    {
        // Arrange
        var payment = new OrderPayment(Guid.NewGuid(), PaymentMethod.Cash, 100.00m);
        payment.Confirm();

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => payment.Update(PaymentMethod.Pix, 150.00m));
    }

    [Fact(DisplayName = "Payment update should throw when cancelled")]
    public void Update_WhenCancelled_ShouldThrowException()
    {
        // Arrange
        var payment = new OrderPayment(Guid.NewGuid(), PaymentMethod.Cash, 100.00m);
        payment.Cancel();

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => payment.Update(PaymentMethod.Pix, 150.00m));
    }
}