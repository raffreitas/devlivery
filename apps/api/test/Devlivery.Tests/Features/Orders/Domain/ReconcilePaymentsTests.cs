using Devlivery.Features.Orders.Domain;
using Devlivery.Features.Orders.Domain.Entities;
using Devlivery.Features.Orders.Domain.Events;
using Devlivery.Features.Orders.Domain.ValueObjects;
using Devlivery.Shared.Domain.Enums;
using Devlivery.Shared.SeedWork;

using Shouldly;

namespace Devlivery.Tests.Features.Orders.Domain;


[Collection("Orders Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class ReconcilePaymentsTests(OrdersUnitTestFixture fixture)
{
    [Fact]
    public void ReconcilePayments_Should_Add_New_Payment_And_Raise_Event()
    {
        var establishmentId = Guid.NewGuid();
        var item = fixture.CreateOrderItem(establishmentId: establishmentId);
        var customer = CustomerInfo.Create("Cliente Teste", null);
        var address = new DeliveryAddress("Rua Teste, 123");

        var existingPayment = new OrderPayment(establishmentId, PaymentMethod.Cash, item.TotalPrice);
        var order = new Order(customer, address, 0m, establishmentId, [item], [existingPayment]);

        var updates = new[] { new OrderPaymentUpdate(existingPayment.Id, PaymentMethod.Cash, existingPayment.Amount), new OrderPaymentUpdate(null, PaymentMethod.Pix, 5m) };

        order.ReconcilePayments(updates);

        order.Payments.Count.ShouldBe(2);
        order.DomainEvents.OfType<OrderPaymentAddedEvent>().Any().ShouldBeTrue();
    }

    [Fact]
    public void ReconcilePayments_Should_Update_Pending_Payment_And_Raise_Event()
    {
        var establishmentId = Guid.NewGuid();
        var item = fixture.CreateOrderItem(establishmentId: establishmentId);
        var customer = CustomerInfo.Create("Cliente Teste", null);
        var address = new DeliveryAddress("Rua Teste, 123");

        var existingPayment = new OrderPayment(establishmentId, PaymentMethod.Cash, item.TotalPrice);
        var order = new Order(customer, address, 0m, establishmentId, [item], [existingPayment]);

        var originalAmount = existingPayment.Amount;

        OrderPaymentUpdate[] updates =
        [
            new OrderPaymentUpdate(existingPayment.Id, PaymentMethod.Pix, originalAmount + 5m)
        ];

        order.ReconcilePayments(updates);

        var payment = order.Payments.First(p => p.Id == existingPayment.Id);
        payment.PaymentMethod.ShouldBe(PaymentMethod.Pix);
        payment.Amount.ShouldBe(originalAmount + 5m);
        order.DomainEvents.OfType<OrderPaymentUpdatedEvent>().Any().ShouldBeTrue();
    }

    [Fact]
    public void ReconcilePayments_Should_Throw_When_Trying_To_Update_Confirmed_Payment()
    {
        var establishmentId = Guid.NewGuid();
        var item = fixture.CreateOrderItem(establishmentId: establishmentId);
        var customer = CustomerInfo.Create("Cliente Teste", null);
        var address = new DeliveryAddress("Rua Teste, 123");

        var existingPayment = new OrderPayment(establishmentId, PaymentMethod.Cash, item.TotalPrice);
        var order = new Order(customer, address, 0m, establishmentId, [item], [existingPayment]);

        // Confirm existing payment
        var paymentRef = order.Payments.First();
        paymentRef.Confirm();

        var updates = new[] { new OrderPaymentUpdate(existingPayment.Id, PaymentMethod.Pix, existingPayment.Amount + 5m) };

        Should.Throw<DomainException>(() => order.ReconcilePayments(updates));
    }

    [Fact]
    public void ReconcilePayments_Should_Cancel_Leftover_Pending_Payments_And_Raise_Event()
    {
        var establishmentId = Guid.NewGuid();
        var item = fixture.CreateOrderItem(establishmentId: establishmentId);
        var customer = CustomerInfo.Create("Cliente Teste", null);
        var address = new DeliveryAddress("Rua Teste, 123");

        var p1 = new OrderPayment(establishmentId, PaymentMethod.Cash, item.TotalPrice / 2);
        var p2 = new OrderPayment(establishmentId, PaymentMethod.Pix, item.TotalPrice / 2);

        var order = new Order(customer, address, 0m, establishmentId, [item], [p1, p2]);

        // Only keep p1 via updates
        var updates = new[] { new OrderPaymentUpdate(p1.Id, p1.PaymentMethod, p1.Amount) };

        order.ReconcilePayments(updates);

        var cancelled = order.DomainEvents.OfType<OrderPaymentCancelledEvent>().Any();
        cancelled.ShouldBeTrue();
        var remaining = order.Payments.First(p => p.Id == p2.Id);
        remaining.PaymentStatus.ShouldBe(Devlivery.Features.Orders.Domain.Enums.PaymentStatus.Cancelled);
    }
}
