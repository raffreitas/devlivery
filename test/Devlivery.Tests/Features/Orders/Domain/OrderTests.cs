using Devlivery.Domain.Aggregates.Orders;
using Devlivery.Domain.Aggregates.Orders.Entities;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.Aggregates.Orders.Events;
using Devlivery.Domain.Aggregates.Orders.ValueObjects;
using Devlivery.Domain.Common.Enums;
using Devlivery.Domain.Common.ValueObjects;
using Devlivery.Domain.SeedWork;

using Shouldly;

namespace Devlivery.Tests.Features.Orders.Domain;

[Collection("Orders Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class OrderTests(OrdersUnitTestFixture fixture)
{
    [Fact]
    public void Constructor_Should_Create_Order_With_Correct_Properties()
    {
        // Arrange
        const string customerName = "João Silva";
        var customerPhone = new PhoneNumber("11987654321");
        var deliveryAddress = new DeliveryAddress("Rua Teste, 123");
        const PaymentMethod paymentMethod = PaymentMethod.Pix;
        const decimal deliveryFee = 10.00m;
        var establishmentId = Guid.NewGuid();
        const string notes = "Sem cebola";
        var customer = CustomerInfo.Create(customerName, customerPhone);
        var item = fixture.CreateOrderItem(establishmentId: establishmentId);

        // Act
        var totalAmount = item.TotalPrice + deliveryFee;
        var payments = new List<OrderPayment> { new(establishmentId, paymentMethod, totalAmount) };

        // Act
        var order = new Order(
            customer: customer,
            deliveryAddress: deliveryAddress,
            deliveryFee: deliveryFee,
            establishmentId: establishmentId,
            items: [item],
            payments: payments,
            notes: notes
        );

        // Assert
        order.Customer.Name.ShouldBe(customerName);
        order.Customer.Phone.ShouldNotBeNull();
        order.Customer.Phone.Number.ShouldBe("11987654321");
        order.DeliveryAddress.FullAddress.ShouldBe("Rua Teste, 123");
        order.Status.ShouldBe(OrderStatus.Pending); // Status sempre começa como Pending
        order.DeliveryFee.ShouldBe(deliveryFee);
        order.EstablishmentId.ShouldBe(establishmentId);
        order.Notes.ShouldBe(notes);
        order.CreatedAt.ShouldNotBe(default);
        order.UpdatedAt.ShouldNotBe(default);
        order.Items.Count.ShouldBe(1);
    }

    [Fact]
    public void Constructor_Should_Calculate_Total_With_Items_And_DeliveryFee()
    {
        // Arrange
        var customer = CustomerInfo.Create("João Silva", new PhoneNumber("11987654321"));
        var deliveryAddress = new DeliveryAddress("Rua Teste, 123");
        var establishmentId = Guid.NewGuid();
        var item1 = fixture.CreateOrderItem(establishmentId: establishmentId, quantity: 2, unitPrice: 50.00m);
        var item2 = fixture.CreateOrderItem(establishmentId: establishmentId, quantity: 1, unitPrice: 30.00m);

        // Act
        var totalAmount = item1.TotalPrice + item2.TotalPrice + 10.00m;
        var payments = new List<OrderPayment> { new(establishmentId, PaymentMethod.Cash, totalAmount) };

        // Act
        var order = new Order(
            customer: customer,
            deliveryAddress: deliveryAddress,
            deliveryFee: 10.00m,
            establishmentId: establishmentId,
            items: [item1, item2],
            payments: payments
        );

        // Assert
        order.Total.ShouldBe(140.00m); // (2 * 50.00) + (1 * 30.00) + 10.00
    }

    [Fact]
    public void Constructor_Should_Throw_When_No_Items_Provided()
    {
        // Arrange
        var customer = CustomerInfo.Create("João Silva");
        var deliveryAddress = new DeliveryAddress("Rua Teste, 123");

        // Act & Assert
        // Act & Assert
        Should.Throw<ArgumentException>(() => new Order(
            customer: customer,
            deliveryAddress: deliveryAddress,
            deliveryFee: 10.00m,
            establishmentId: Guid.NewGuid(),
            items: [],
            payments: [new(Guid.NewGuid(), PaymentMethod.Cash, 10.00m)]
        ));
    }

    [Fact]
    public void Constructor_Should_Throw_When_DeliveryFee_Is_Negative()
    {
        // Arrange
        var customer = CustomerInfo.Create("João Silva");
        var deliveryAddress = new DeliveryAddress("Rua Teste, 123");
        var establishmentId = Guid.NewGuid();
        var item = fixture.CreateOrderItem(establishmentId: establishmentId);

        // Act & Assert
        // Act & Assert
        Should.Throw<ArgumentException>(() => new Order(
            customer: customer,
            deliveryAddress: deliveryAddress,
            deliveryFee: -5.00m,
            establishmentId: establishmentId,
            items: [item],
            payments: [new(establishmentId, PaymentMethod.Cash, item.TotalPrice - 5.00m)]
        ));
    }

    [Fact]
    public void UpdateStatus_Should_Change_Status()
    {
        // Arrange
        var order = fixture.CreateOrder();

        // Act
        order.UpdateStatus(OrderStatus.Preparing);

        // Assert
        order.Status.ShouldBe(OrderStatus.Preparing);
    }

    [Fact]
    public async Task UpdateStatus_Should_Update_UpdatedAt_Timestamp()
    {
        // Arrange
        var order = fixture.CreateOrder();
        var originalUpdatedAt = order.UpdatedAt;
        await Task.Delay(10);

        // Act
        order.UpdateStatus(OrderStatus.Ready);

        // Assert
        order.UpdatedAt.ShouldBeGreaterThan(originalUpdatedAt);
    }

    [Fact]
    public void UpdateStatus_Should_Raise_OrderStatusChangedEvent()
    {
        // Arrange
        var order = fixture.CreateOrder();

        // Act
        order.UpdateStatus(OrderStatus.Delivered);

        // Assert
        var statusChangedEvent = order.DomainEvents.OfType<OrderStatusChangedEvent>().FirstOrDefault();
        statusChangedEvent.ShouldNotBeNull();
        statusChangedEvent.OldStatus.ShouldBe(OrderStatus.Pending);
        statusChangedEvent.NewStatus.ShouldBe(OrderStatus.Delivered);
    }

    [Fact]
    public void UpdateStatus_Should_Throw_When_Order_Is_Canceled()
    {
        // Arrange
        var order = fixture.CreateOrder(status: OrderStatus.Canceled);

        // Act & Assert
        Should.Throw<DomainException>(() => order.UpdateStatus(OrderStatus.Preparing));
    }

    [Fact]
    public void UpdateStatus_Should_Throw_When_Order_Is_Delivered()
    {
        // Arrange
        var order = fixture.CreateOrder(status: OrderStatus.Delivered);

        // Act & Assert
        Should.Throw<DomainException>(() => order.UpdateStatus(OrderStatus.Preparing));
    }

    [Fact]
    public void UpdateDetails_Should_Update_Customer_Information()
    {
        // Arrange
        var order = fixture.CreateOrder(
            customerName: "Nome Original",
            customerPhone: "11111111111",
            deliveryAddress: "Endereço Original",
            paymentMethod: PaymentMethod.Cash,
            deliveryFee: 5.00m,
            notes: "Notas Originais"
        );

        var newCustomer = CustomerInfo.Create("Nome Atualizado", new PhoneNumber("22222222222"));
        var newDeliveryAddress = new DeliveryAddress("Endereço Atualizado");

        // Act
        order.UpdateDetails(
            customer: newCustomer,
            deliveryAddress: newDeliveryAddress,
            deliveryFee: 10.00m,
            notes: "Notas Atualizadas"
        );

        // Assert
        order.Customer.Name.ShouldBe("Nome Atualizado");
        order.Customer.Phone.ShouldNotBeNull();
        order.Customer.Phone.Number.ShouldBe("22222222222");
        order.DeliveryAddress.FullAddress.ShouldBe("Endereço Atualizado");
        order.Payments.First().PaymentMethod.ShouldBe(PaymentMethod.Cash); // Não muda
        order.DeliveryFee.ShouldBe(10.00m);
        order.Notes.ShouldBe("Notas Atualizadas");
    }

    [Fact]
    public void UpdateDetails_Should_Recalculate_Total_When_DeliveryFee_Changes()
    {
        // Arrange
        var establishmentId = Guid.NewGuid();
        var item = fixture.CreateOrderItem(quantity: 2, unitPrice: 50.00m, establishmentId: establishmentId);
        var customer = CustomerInfo.Create("João Silva");
        var deliveryAddress = new DeliveryAddress("Rua Teste, 123");

        var totalAmount = item.TotalPrice + 5.00m;
        var payments = new List<OrderPayment> { new(establishmentId, PaymentMethod.Cash, totalAmount) };

        var order = new Order(
            customer: customer,
            deliveryAddress: deliveryAddress,
            deliveryFee: 5.00m,
            establishmentId: establishmentId,
            items: [item],
            payments: payments
        );

        // Total inicial: (2 * 50.00) + 5.00 = 105.00
        order.Total.ShouldBe(105.00m);

        // Act
        order.UpdateDetails(
            customer: customer,
            deliveryAddress: deliveryAddress,
            deliveryFee: 15.00m
        );

        // Assert
        order.Total.ShouldBe(115.00m); // (2 * 50.00) + 15.00
    }

    [Fact]
    public void UpdateDetails_Should_Replace_Items_When_Provided()
    {
        // Arrange
        var establishmentId = Guid.NewGuid();
        var originalItem = fixture.CreateOrderItem(establishmentId: establishmentId);
        var customer = CustomerInfo.Create("João Silva");
        var deliveryAddress = new DeliveryAddress("Rua Teste, 123");

        var totalAmount = originalItem.TotalPrice + 5.00m;
        var payments = new List<OrderPayment> { new(establishmentId, PaymentMethod.Cash, totalAmount) };

        var order = new Order(
            customer: customer,
            deliveryAddress: deliveryAddress,
            deliveryFee: 5.00m,
            establishmentId: establishmentId,
            items: [originalItem],
            payments: payments
        );

        var newItem1 = fixture.CreateOrderItem(quantity: 2, unitPrice: 25.00m, establishmentId: establishmentId);
        var newItem2 = fixture.CreateOrderItem(quantity: 1, unitPrice: 40.00m, establishmentId: establishmentId);

        // Act
        order.UpdateDetails(
            customer: customer,
            deliveryAddress: deliveryAddress,
            deliveryFee: 10.00m,
            items: [newItem1, newItem2]
        );

        // Assert
        order.Items.Count.ShouldBe(2);
        order.Items.ShouldNotContain(originalItem);
        order.Total.ShouldBe(100.00m); // (2 * 25.00) + (1 * 40.00) + 10.00
    }

    [Fact]
    public async Task UpdateDetails_Should_Update_UpdatedAt_Timestamp()
    {
        // Arrange
        var order = fixture.CreateOrder();
        var originalUpdatedAt = order.UpdatedAt;
        await Task.Delay(10);

        var newCustomer = CustomerInfo.Create("Novo Nome");
        var newDeliveryAddress = new DeliveryAddress("Novo Endereço");

        // Act
        order.UpdateDetails(
            customer: newCustomer,
            deliveryAddress: newDeliveryAddress,
            deliveryFee: 12.00m
        );

        // Assert
        order.UpdatedAt.ShouldBeGreaterThan(originalUpdatedAt);
    }

    [Fact]
    public void UpdatePaymentMethod_Should_Change_PaymentMethod()
    {
        // Arrange
        var order = fixture.CreateOrder(paymentMethod: PaymentMethod.Cash);

        // Act
        order.AddPayment(new OrderPayment(order.EstablishmentId, PaymentMethod.Pix, order.Total));

        // Assert
        order.Payments.Any(p => p.PaymentMethod == PaymentMethod.Pix).ShouldBeTrue();
    }

    [Fact]
    public void Delete_Should_Raise_OrderDeletedEvent()
    {
        // Arrange
        var order = fixture.CreateOrder();

        // Act
        order.Delete();

        // Assert
        var deletedEvent = order.DomainEvents.OfType<OrderDeletedEvent>().FirstOrDefault();
        deletedEvent.ShouldNotBeNull();
        deletedEvent.OrderId.ShouldBe(order.Id);
        deletedEvent.EstablishmentId.ShouldBe(order.EstablishmentId);
    }

    [Fact]
    public void Total_Should_Be_Sum_Of_Items_Plus_DeliveryFee()
    {
        // Arrange
        var establishmentId = Guid.NewGuid();
        var item1 = fixture.CreateOrderItem(quantity: 2, unitPrice: 30.00m, establishmentId: establishmentId); // 60.00
        var item2 = fixture.CreateOrderItem(quantity: 3, unitPrice: 15.00m, establishmentId: establishmentId); // 45.00
        var item3 = fixture.CreateOrderItem(quantity: 1, unitPrice: 25.00m, establishmentId: establishmentId); // 25.00
        var customer = CustomerInfo.Create("João Silva");
        var deliveryAddress = new DeliveryAddress("Rua Teste, 123");

        // Act
        var totalAmount = item1.TotalPrice + item2.TotalPrice + item3.TotalPrice + 7.50m;
        var payments = new List<OrderPayment> { new(establishmentId, PaymentMethod.Cash, totalAmount) };

        // Act
        var order = new Order(
            customer: customer,
            deliveryAddress: deliveryAddress,
            deliveryFee: 7.50m,
            establishmentId: establishmentId,
            items: [item1, item2, item3],
            payments: payments
        );

        // Assert
        order.Total.ShouldBe(137.50m); // 60.00 + 45.00 + 25.00 + 7.50
    }
}