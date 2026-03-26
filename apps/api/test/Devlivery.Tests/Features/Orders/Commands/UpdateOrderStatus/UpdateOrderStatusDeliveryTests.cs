using Devlivery.Domain.Aggregates.Orders;
using Devlivery.Domain.Aggregates.Orders.Entities;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.Aggregates.Orders.Events;
using Devlivery.Domain.Aggregates.Orders.ValueObjects;
using Devlivery.Domain.Common.Enums;
using Devlivery.Domain.SeedWork;

using Shouldly;

namespace Devlivery.Tests.Features.Orders.Commands.UpdateOrderStatus;

[Collection("Orders Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class UpdateOrderStatusDeliveryTests(OrdersUnitTestFixture fixture)
{
    [Fact]
    public void UpdateStatus_To_Delivered_Should_Confirm_Payments_And_Calculate_Change()
    {
        var establishmentId = Guid.NewGuid();
        var item = fixture.CreateOrderItem(establishmentId: establishmentId, quantity: 1, unitPrice: 20.00m);
        var customer = CustomerInfo.Create("Cliente Teste", null);
        var address = new DeliveryAddress("Rua Teste, 123");

        // payments sum greater than total
        var p1 = new OrderPayment(establishmentId, PaymentMethod.Cash, 30m);

        var order = new Order(customer, address, 0m, establishmentId, [item], [p1]);

        order.UpdateStatus(OrderStatus.Delivered);

        order.Change.ShouldBe(10m); // 30 - 20
        order.Payments.All(p => p.PaymentStatus == PaymentStatus.Confirmed).ShouldBeTrue();
        order.DomainEvents.OfType<OrderPaymentConfirmedEvent>().Any().ShouldBeTrue();
        order.DomainEvents.OfType<OrderChangeCalculatedEvent>().Any().ShouldBeTrue();
    }

    [Fact]
    public void UpdateStatus_To_Delivered_Should_Throw_When_Payments_Insufficient()
    {
        var establishmentId = Guid.NewGuid();
        var item = fixture.CreateOrderItem(establishmentId: establishmentId, quantity: 1, unitPrice: 50.00m);
        var customer = CustomerInfo.Create("Cliente Teste", null);
        var address = new DeliveryAddress("Rua Teste, 123");

        var p1 = new OrderPayment(establishmentId, PaymentMethod.Cash, 20m);

        var order = new Order(customer, address, 0m, establishmentId, [item], [p1]);

        Should.Throw<DomainException>(() => order.UpdateStatus(OrderStatus.Delivered));
    }
}