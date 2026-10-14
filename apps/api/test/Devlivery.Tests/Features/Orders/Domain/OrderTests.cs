using Devlivery.Domain.Aggregates.Orders;
using Devlivery.Domain.Aggregates.Orders.Entities;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.Aggregates.Orders.Events;
using Devlivery.Domain.Aggregates.Orders.ValueObjects;
using Devlivery.Domain.Common.Enums;
using Devlivery.Domain.SeedWork;
using Devlivery.Tests.Common.Builders;

using Shouldly;

namespace Devlivery.Tests.Features.Orders.Domain;

[Trait("Category", "Unit Tests")]
public sealed class OrderTests
{
    private static readonly CustomerInfo Customer = CustomerInfo.Create("João Silva", "11987654321");
    private static readonly DeliveryAddress Address = new("Rua das Flores, 123", "Portão azul");

    [Fact]
    public void GivenItemsAndDeliveryFee_WhenCreatingOrder_ThenStartsPendingWithTotalOfItemsPlusFee()
    {
        // Arrange
        var establishmentId = Guid.NewGuid();
        var items = new List<OrderItem>
        {
            new OrderItemBuilder().WithQuantity(2).WithUnitPrice(50m).Build(),
            new OrderItemBuilder().WithQuantity(1).WithUnitPrice(30m).Build()
        };
        var payments = new List<OrderPayment> { new(establishmentId, PaymentMethod.Pix, 140m) };

        // Act
        var order = new Order(Customer, Address, 10m, establishmentId, items, payments, "Sem cebola");

        // Assert
        order.Status.ShouldBe(OrderStatus.Pending);
        order.Total.ShouldBe(140m);
        order.Change.ShouldBe(0m);
        order.Customer.ShouldBe(Customer);
        order.DeliveryAddress.ShouldBe(Address);
        order.EstablishmentId.ShouldBe(establishmentId);
        order.Notes.ShouldBe("Sem cebola");
        order.Items.Count.ShouldBe(2);
        order.Payments.ShouldHaveSingleItem().PaymentStatus.ShouldBe(PaymentStatus.Pending);
    }

    [Fact]
    public void GivenNoItems_WhenCreatingOrder_ThenThrowsDomainException()
    {
        // Arrange
        var payments = new List<OrderPayment> { new OrderPaymentBuilder().Build() };

        // Act
        var exception = Record.Exception(() => new Order(Customer, Address, 0m, Guid.NewGuid(), [], payments));

        // Assert
        exception.ShouldBeOfType<DomainException>().Message.ShouldBe("Pedido deve ter pelo menos um item");
    }

    [Fact]
    public void GivenNoPayments_WhenCreatingOrder_ThenThrowsDomainException()
    {
        // Arrange
        var items = new List<OrderItem> { new OrderItemBuilder().Build() };

        // Act
        var exception = Record.Exception(() => new Order(Customer, Address, 0m, Guid.NewGuid(), items, []));

        // Assert
        exception.ShouldBeOfType<DomainException>()
            .Message.ShouldBe("Pedido deve ter pelo menos uma forma de pagamento");
    }

    [Fact]
    public void GivenNegativeDeliveryFee_WhenCreatingOrder_ThenThrowsDomainException()
    {
        // Arrange
        var items = new List<OrderItem> { new OrderItemBuilder().Build() };
        var payments = new List<OrderPayment> { new OrderPaymentBuilder().Build() };

        // Act
        var exception = Record.Exception(() => new Order(Customer, Address, -5m, Guid.NewGuid(), items, payments));

        // Assert
        exception.ShouldBeOfType<DomainException>().Message.ShouldBe("Taxa de entrega não pode ser negativa");
    }

    [Fact]
    public void GivenPendingOrder_WhenChangingStatus_ThenRaisesStatusChangedEvent()
    {
        // Arrange
        var order = new OrderBuilder().Build();
        var beforeUpdate = DateTime.UtcNow;

        // Act
        order.UpdateStatus(OrderStatus.Preparing);

        // Assert
        order.Status.ShouldBe(OrderStatus.Preparing);
        order.UpdatedAt.ShouldBeGreaterThanOrEqualTo(beforeUpdate);
        var statusChanged = order.DomainEvents.OfType<OrderStatusChangedEvent>().ShouldHaveSingleItem();
        statusChanged.OrderId.ShouldBe(order.Id);
        statusChanged.OldStatus.ShouldBe(OrderStatus.Pending);
        statusChanged.NewStatus.ShouldBe(OrderStatus.Preparing);
    }

    [Theory]
    [InlineData(OrderStatus.Canceled, OrderStatus.Preparing)]
    [InlineData(OrderStatus.Canceled, OrderStatus.Delivered)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Preparing)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Canceled)]
    public void GivenFinalizedOrder_WhenChangingStatus_ThenThrowsDomainException(
        OrderStatus finalStatus, OrderStatus newStatus)
    {
        // Arrange
        var order = new OrderBuilder().WithStatus(finalStatus).Build();

        // Act
        var exception = Record.Exception(() => order.UpdateStatus(newStatus));

        // Assert
        exception.ShouldBeOfType<DomainException>();
        order.Status.ShouldBe(finalStatus);
    }

    [Fact]
    public void GivenPaymentAboveTotal_WhenDelivering_ThenConfirmsPaymentAndCalculatesChange()
    {
        // Arrange
        var payment = new OrderPaymentBuilder().WithPaymentMethod(PaymentMethod.Cash).WithAmount(100m).Build();
        var order = CreateOrderWithTotal(70m, payment);

        // Act
        order.UpdateStatus(OrderStatus.Delivered);

        // Assert
        order.Status.ShouldBe(OrderStatus.Delivered);
        order.Change.ShouldBe(30m);
        payment.PaymentStatus.ShouldBe(PaymentStatus.Confirmed);
        payment.ConfirmedAt.ShouldNotBeNull();

        var paymentConfirmed = order.DomainEvents.OfType<OrderPaymentConfirmedEvent>().ShouldHaveSingleItem();
        paymentConfirmed.PaymentId.ShouldBe(payment.Id);
        paymentConfirmed.PaymentMethod.ShouldBe(PaymentMethod.Cash);
        paymentConfirmed.Amount.ShouldBe(100m);
        paymentConfirmed.OrderTotal.ShouldBe(70m);
        order.DomainEvents.OfType<OrderChangeCalculatedEvent>().ShouldHaveSingleItem().Change.ShouldBe(30m);
    }

    [Fact]
    public void GivenPaymentsBelowTotal_WhenDelivering_ThenThrowsAndKeepsOrderOpen()
    {
        // Arrange
        var payment = new OrderPaymentBuilder().WithAmount(20m).Build();
        var order = CreateOrderWithTotal(50m, payment);

        // Act
        var exception = Record.Exception(() => order.UpdateStatus(OrderStatus.Delivered));

        // Assert
        exception.ShouldBeOfType<DomainException>();
        order.Status.ShouldBe(OrderStatus.Pending);
        payment.PaymentStatus.ShouldBe(PaymentStatus.Pending);
    }

    [Fact]
    public void GivenCancelledPaymentNeededToCoverTotal_WhenDelivering_ThenThrowsDomainException()
    {
        // Arrange
        var pending = new OrderPaymentBuilder().WithAmount(50m).Build();
        var cancelled = new OrderPaymentBuilder().WithAmount(50m).AsCancelled().Build();
        var order = CreateOrderWithTotal(100m, pending, cancelled);

        // Act
        var exception = Record.Exception(() => order.UpdateStatus(OrderStatus.Delivered));

        // Assert
        exception.ShouldBeOfType<DomainException>();
    }

    [Fact]
    public void GivenMixedPaymentStatuses_WhenDelivering_ThenConfirmsOnlyPendingPayments()
    {
        // Arrange
        var alreadyConfirmed = new OrderPaymentBuilder().WithAmount(40m).AsConfirmed().Build();
        var pendingCash = new OrderPaymentBuilder().WithPaymentMethod(PaymentMethod.Cash).WithAmount(30m).Build();
        var pendingPix = new OrderPaymentBuilder().WithPaymentMethod(PaymentMethod.Pix).WithAmount(30m).Build();
        var cancelled = new OrderPaymentBuilder().WithAmount(25m).AsCancelled().Build();
        var order = CreateOrderWithTotal(100m, alreadyConfirmed, pendingCash, pendingPix, cancelled);

        // Act
        order.UpdateStatus(OrderStatus.Delivered);

        // Assert
        order.DomainEvents.OfType<OrderPaymentConfirmedEvent>().Select(e => e.PaymentId)
            .ShouldBe([pendingCash.Id, pendingPix.Id], ignoreOrder: true);
        pendingCash.PaymentStatus.ShouldBe(PaymentStatus.Confirmed);
        pendingPix.PaymentStatus.ShouldBe(PaymentStatus.Confirmed);
        cancelled.PaymentStatus.ShouldBe(PaymentStatus.Cancelled);
        order.Change.ShouldBe(0m);
    }

    [Fact]
    public void GivenDeliveredOrder_WhenDeliveringAgain_ThenDoesNotConfirmPaymentsAgain()
    {
        // Arrange
        var payment = new OrderPaymentBuilder().WithAmount(50m).Build();
        var order = CreateOrderWithTotal(50m, payment);
        order.UpdateStatus(OrderStatus.Delivered);

        // Act
        var exception = Record.Exception(() => order.UpdateStatus(OrderStatus.Delivered));

        // Assert
        exception.ShouldBeNull();
        order.DomainEvents.OfType<OrderPaymentConfirmedEvent>().ShouldHaveSingleItem();
        payment.PaymentStatus.ShouldBe(PaymentStatus.Confirmed);
    }

    [Fact]
    public void GivenOrder_WhenUpdatingDetailsWithNewItems_ThenReplacesItemsAndRecalculatesTotal()
    {
        // Arrange
        var originalItem = new OrderItemBuilder().Build();
        var order = new OrderBuilder().WithItems(originalItem).WithDeliveryFee(5m).Build();
        var newCustomer = CustomerInfo.Create("Maria Souza", "21912345678");
        var newAddress = new DeliveryAddress("Av. Brasil, 500");
        var newItems = new List<OrderItem>
        {
            new OrderItemBuilder().WithQuantity(2).WithUnitPrice(25m).Build(),
            new OrderItemBuilder().WithQuantity(1).WithUnitPrice(40m).Build()
        };
        var beforeUpdate = DateTime.UtcNow;

        // Act
        order.UpdateDetails(newCustomer, newAddress, 10m, "Troco para 200", newItems);

        // Assert
        order.Customer.ShouldBe(newCustomer);
        order.DeliveryAddress.ShouldBe(newAddress);
        order.DeliveryFee.ShouldBe(10m);
        order.Notes.ShouldBe("Troco para 200");
        order.Items.ShouldBe(newItems, ignoreOrder: true);
        order.Items.ShouldNotContain(originalItem);
        order.Total.ShouldBe(100m);
        order.UpdatedAt.ShouldBeGreaterThanOrEqualTo(beforeUpdate);
    }

    [Fact]
    public void GivenOrder_WhenUpdatingDetailsWithoutItems_ThenKeepsItemsAndRecalculatesTotalWithNewFee()
    {
        // Arrange
        var item = new OrderItemBuilder().WithQuantity(2).WithUnitPrice(50m).Build();
        var order = new OrderBuilder().WithItems(item).WithDeliveryFee(5m).Build();

        // Act
        order.UpdateDetails(Customer, Address, 15m);

        // Assert
        order.Items.ShouldHaveSingleItem().ShouldBe(item);
        order.Total.ShouldBe(115m);
    }

    [Fact]
    public void GivenOrder_WhenDeleting_ThenRaisesOrderDeletedEvent()
    {
        // Arrange
        var order = new OrderBuilder().WithStatus(OrderStatus.Preparing).Build();

        // Act
        order.Delete();

        // Assert
        var deleted = order.DomainEvents.OfType<OrderDeletedEvent>().ShouldHaveSingleItem();
        deleted.OrderId.ShouldBe(order.Id);
        deleted.EstablishmentId.ShouldBe(order.EstablishmentId);
        deleted.Total.ShouldBe(order.Total);
        deleted.Status.ShouldBe(OrderStatus.Preparing);
    }

    private static Order CreateOrderWithTotal(decimal total, params OrderPayment[] payments)
    {
        return new OrderBuilder()
            .WithItems(new OrderItemBuilder().WithQuantity(1).WithUnitPrice(total).Build())
            .WithDeliveryFee(0m)
            .WithCustomPayments(payments)
            .Build();
    }
}