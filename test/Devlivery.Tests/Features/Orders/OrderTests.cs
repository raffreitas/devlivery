using Devlivery.Features.Orders.Domain;
using Devlivery.Features.Orders.Domain.Events;

using Shouldly;

namespace Devlivery.Tests.Features.Orders;

[Collection("Orders Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class OrderTests
{
    private readonly OrdersUnitTestFixture _fixture;

    public OrderTests(OrdersUnitTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Constructor_Should_Create_Order_With_Correct_Properties()
    {
        // Arrange
        var customerName = "João Silva";
        var customerPhone = "11987654321";
        var deliveryAddress = "Rua Teste, 123";
        var paymentMethod = PaymentMethod.Pix;
        var status = OrderStatus.Pending;
        var deliveryFee = 10.00m;
        var establishmentId = Guid.NewGuid();
        var notes = "Sem cebola";

        // Act
        var order = new Order(
            customerName,
            customerPhone,
            deliveryAddress,
            paymentMethod,
            status,
            deliveryFee,
            establishmentId,
            notes
        );

        // Assert
        order.CustomerName.ShouldBe(customerName);
        order.CustomerPhone.ShouldBe(customerPhone);
        order.DeliveryAddress.ShouldBe(deliveryAddress);
        order.PaymentMethod.ShouldBe(paymentMethod);
        order.Status.ShouldBe(status);
        order.DeliveryFee.ShouldBe(deliveryFee);
        order.EstablishmentId.ShouldBe(establishmentId);
        order.Notes.ShouldBe(notes);
        order.CreatedAt.ShouldNotBe(default);
        order.UpdatedAt.ShouldNotBe(default);
        order.Items.ShouldBeEmpty();
    }

    [Fact]
    public void AddItem_Should_Add_Item_To_Order()
    {
        // Arrange
        var order = _fixture.CreateOrder();
        var orderItem = _fixture.CreateOrderItem();

        // Act
        order.AddItem(orderItem);

        // Assert
        order.Items.Count.ShouldBe(1);
        order.Items.ShouldContain(orderItem);
    }

    [Fact]
    public void AddItem_Should_Update_Total()
    {
        // Arrange
        var order = _fixture.CreateOrder(deliveryFee: 10.00m);
        var orderItem = _fixture.CreateOrderItem(quantity: 2, unitPrice: 50.00m);

        // Act
        order.AddItem(orderItem);

        // Assert
        order.Total.ShouldBe(110.00m); // (2 * 50.00) + 10.00
    }

    [Fact]
    public async Task AddItem_Should_Update_UpdatedAt_Timestamp()
    {
        // Arrange
        var order = _fixture.CreateOrder();
        var originalUpdatedAt = order.UpdatedAt;
        await Task.Delay(10); // Garante que o timestamp será diferente

        var orderItem = _fixture.CreateOrderItem();

        // Act
        order.AddItem(orderItem);

        // Assert
        order.UpdatedAt.ShouldBeGreaterThan(originalUpdatedAt);
    }

    [Fact]
    public void AddItem_Should_Add_Multiple_Items()
    {
        // Arrange
        var order = _fixture.CreateOrder(deliveryFee: 5.00m);
        var item1 = _fixture.CreateOrderItem(quantity: 1, unitPrice: 20.00m);
        var item2 = _fixture.CreateOrderItem(quantity: 2, unitPrice: 15.00m);

        // Act
        order.AddItem(item1);
        order.AddItem(item2);

        // Assert
        order.Items.Count.ShouldBe(2);
        order.Total.ShouldBe(55.00m); // (1 * 20.00) + (2 * 15.00) + 5.00
    }

    [Fact]
    public void ReplaceItems_Should_Clear_Existing_Items()
    {
        // Arrange
        var order = _fixture.CreateOrder();
        var oldItem = _fixture.CreateOrderItem();
        order.AddItem(oldItem);

        var newItems = new[] { _fixture.CreateOrderItem(quantity: 1, unitPrice: 30.00m) };

        // Act
        order.ReplaceItems(newItems);

        // Assert
        order.Items.Count.ShouldBe(1);
        order.Items.ShouldNotContain(oldItem);
    }

    [Fact]
    public void ReplaceItems_Should_Add_New_Items()
    {
        // Arrange
        var order = _fixture.CreateOrder(deliveryFee: 10.00m);
        var item1 = _fixture.CreateOrderItem(quantity: 2, unitPrice: 25.00m);
        var item2 = _fixture.CreateOrderItem(quantity: 1, unitPrice: 40.00m);

        var newItems = new[] { item1, item2 };

        // Act
        order.ReplaceItems(newItems);

        // Assert
        order.Items.Count.ShouldBe(2);
        order.Items.ShouldContain(item1);
        order.Items.ShouldContain(item2);
        order.Total.ShouldBe(100.00m); // (2 * 25.00) + (1 * 40.00) + 10.00
    }

    [Fact]
    public async Task ReplaceItems_Should_Update_UpdatedAt_Timestamp()
    {
        // Arrange
        var order = _fixture.CreateOrder();
        var originalUpdatedAt = order.UpdatedAt;
        await Task.Delay(10);

        var newItems = new[] { _fixture.CreateOrderItem() };

        // Act
        order.ReplaceItems(newItems);

        // Assert
        order.UpdatedAt.ShouldBeGreaterThan(originalUpdatedAt);
    }

    [Fact]
    public void ReplaceItems_Should_Raise_OrderUpdatedEvent()
    {
        // Arrange
        var order = _fixture.CreateOrder();
        var newItems = new[] { _fixture.CreateOrderItem() };

        // Act
        order.ReplaceItems(newItems);

        // Assert
        order.DomainEvents.ShouldContain(e => e is OrderUpdatedEvent);
    }

    [Fact]
    public void UpdateStatus_Should_Change_Status()
    {
        // Arrange
        var order = _fixture.CreateOrder(status: OrderStatus.Pending);

        // Act
        order.UpdateStatus(OrderStatus.Preparing);

        // Assert
        order.Status.ShouldBe(OrderStatus.Preparing);
    }

    [Fact]
    public async Task UpdateStatus_Should_Update_UpdatedAt_Timestamp()
    {
        // Arrange
        var order = _fixture.CreateOrder();
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
        var order = _fixture.CreateOrder(status: OrderStatus.Pending);

        // Act
        order.UpdateStatus(OrderStatus.Delivered);

        // Assert
        var statusChangedEvent = order.DomainEvents.OfType<OrderStatusChangedEvent>().FirstOrDefault();
        statusChangedEvent.ShouldNotBeNull();
        statusChangedEvent.OldStatus.ShouldBe(OrderStatus.Pending);
        statusChangedEvent.NewStatus.ShouldBe(OrderStatus.Delivered);
    }

    [Fact]
    public void UpdateDetails_Should_Update_Customer_Information()
    {
        // Arrange
        var order = _fixture.CreateOrder(
            customerName: "Nome Original",
            customerPhone: "11111111111",
            deliveryAddress: "Endereço Original",
            paymentMethod: PaymentMethod.Cash,
            deliveryFee: 5.00m,
            notes: "Notas Originais"
        );

        // Act
        order.UpdateDetails(
            customerName: "Nome Atualizado",
            customerPhone: "22222222222",
            deliveryAddress: "Endereço Atualizado",
            paymentMethod: PaymentMethod.Pix,
            deliveryFee: 10.00m,
            notes: "Notas Atualizadas"
        );

        // Assert
        order.CustomerName.ShouldBe("Nome Atualizado");
        order.CustomerPhone.ShouldBe("22222222222");
        order.DeliveryAddress.ShouldBe("Endereço Atualizado");
        order.PaymentMethod.ShouldBe(PaymentMethod.Pix);
        order.DeliveryFee.ShouldBe(10.00m);
        order.Notes.ShouldBe("Notas Atualizadas");
    }

    [Fact]
    public void UpdateDetails_Should_Recalculate_Total_When_DeliveryFee_Changes()
    {
        // Arrange
        var order = _fixture.CreateOrder(deliveryFee: 5.00m);
        var item = _fixture.CreateOrderItem(quantity: 2, unitPrice: 50.00m);
        order.AddItem(item);

        // Total inicial: (2 * 50.00) + 5.00 = 105.00

        // Act
        order.UpdateDetails(
            customerName: order.CustomerName,
            customerPhone: order.CustomerPhone,
            deliveryAddress: order.DeliveryAddress,
            paymentMethod: order.PaymentMethod,
            deliveryFee: 15.00m
        );

        // Assert
        order.Total.ShouldBe(115.00m); // (2 * 50.00) + 15.00
    }

    [Fact]
    public async Task UpdateDetails_Should_Update_UpdatedAt_Timestamp()
    {
        // Arrange
        var order = _fixture.CreateOrder();
        var originalUpdatedAt = order.UpdatedAt;
        await Task.Delay(10);

        // Act
        order.UpdateDetails(
            customerName: "Novo Nome",
            customerPhone: null,
            deliveryAddress: "Novo Endereço",
            paymentMethod: PaymentMethod.CreditCard,
            deliveryFee: 12.00m
        );

        // Assert
        order.UpdatedAt.ShouldBeGreaterThan(originalUpdatedAt);
    }

    [Fact]
    public void UpdateDetails_Should_Raise_OrderUpdatedEvent()
    {
        // Arrange
        var order = _fixture.CreateOrder();

        // Act
        order.UpdateDetails(
            customerName: "Novo Nome",
            customerPhone: null,
            deliveryAddress: "Novo Endereço",
            paymentMethod: PaymentMethod.DebitCard,
            deliveryFee: 8.00m
        );

        // Assert
        order.DomainEvents.ShouldContain(e => e is OrderUpdatedEvent);
    }

    [Fact]
    public void RaiseCreatedEvent_Should_Add_OrderCreatedEvent_To_DomainEvents()
    {
        // Arrange
        var order = _fixture.CreateOrder();

        // Act
        order.RaiseCreatedEvent();

        // Assert
        var createdEvent = order.DomainEvents.OfType<OrderCreatedEvent>().FirstOrDefault();
        createdEvent.ShouldNotBeNull();
        createdEvent.OrderId.ShouldBe(order.Id);
        createdEvent.EstablishmentId.ShouldBe(order.EstablishmentId);
        createdEvent.CustomerName.ShouldBe(order.CustomerName);
    }

    [Fact]
    public void Total_Should_Be_Sum_Of_Items_Plus_DeliveryFee()
    {
        // Arrange
        var order = _fixture.CreateOrder(deliveryFee: 7.50m);
        var item1 = _fixture.CreateOrderItem(quantity: 2, unitPrice: 30.00m); // 60.00
        var item2 = _fixture.CreateOrderItem(quantity: 3, unitPrice: 15.00m); // 45.00
        var item3 = _fixture.CreateOrderItem(quantity: 1, unitPrice: 25.00m); // 25.00

        // Act
        order.AddItem(item1);
        order.AddItem(item2);
        order.AddItem(item3);

        // Assert
        order.Total.ShouldBe(137.50m); // 60.00 + 45.00 + 25.00 + 7.50
    }

    [Fact]
    public void Total_Should_Be_Zero_When_No_Items_Before_Calculation()
    {
        // Arrange
        var order = _fixture.CreateOrder(deliveryFee: 12.00m);

        // Assert - Total é calculado apenas quando itens são adicionados ou UpdateDetails é chamado
        order.Total.ShouldBe(0m);
    }
}