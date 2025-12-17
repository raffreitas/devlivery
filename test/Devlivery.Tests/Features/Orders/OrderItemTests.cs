using Devlivery.Features.Orders.Domain.Entities;

using Shouldly;

namespace Devlivery.Tests.Features.Orders;

[Collection("Orders Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class OrderItemTests(OrdersUnitTestFixture fixture)
{
    [Fact]
    public void Constructor_Should_Create_OrderItem_With_Correct_Properties()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var establishmentId = Guid.NewGuid();
        const int quantity = 3;
        const decimal unitPrice = 25.50m;
        const string notes = "Sem gelo";

        // Act
        var orderItem = new OrderItem(productId, establishmentId, quantity, unitPrice, notes);

        // Assert
        orderItem.ProductId.ShouldBe(productId);
        orderItem.EstablishmentId.ShouldBe(establishmentId);
        orderItem.Quantity.ShouldBe(quantity);
        orderItem.UnitPrice.ShouldBe(unitPrice);
        orderItem.Notes.ShouldBe(notes);
    }

    [Fact]
    public void TotalPrice_Should_Be_UnitPrice_Multiplied_By_Quantity()
    {
        // Arrange
        var orderItem = fixture.CreateOrderItem(quantity: 5, unitPrice: 12.00m);

        // Act
        var totalPrice = orderItem.TotalPrice;

        // Assert
        totalPrice.ShouldBe(60.00m); // 5 * 12.00
    }

    [Theory]
    [InlineData(1, 10.00, 10.00)]
    [InlineData(2, 15.50, 31.00)]
    [InlineData(10, 7.99, 79.90)]
    [InlineData(3, 100.00, 300.00)]
    public void TotalPrice_Should_Calculate_Correctly_For_Various_Values(int quantity, decimal unitPrice,
        decimal expectedTotal)
    {
        // Arrange
        var orderItem = fixture.CreateOrderItem(quantity: quantity, unitPrice: unitPrice);

        // Act
        var totalPrice = orderItem.TotalPrice;

        // Assert
        totalPrice.ShouldBe(expectedTotal);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Quantity_Is_Zero()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() =>
                fixture.CreateOrderItem(quantity: 0, unitPrice: 50.00m))
            .Message.ShouldContain("Quantidade deve ser maior que zero");
    }

    [Fact]
    public void TotalPrice_Should_Handle_Decimal_Precision()
    {
        // Arrange
        var orderItem = fixture.CreateOrderItem(quantity: 3, unitPrice: 12.33m);

        // Act
        var totalPrice = orderItem.TotalPrice;

        // Assert
        totalPrice.ShouldBe(36.99m); // 3 * 12.33
    }

    [Fact]
    public void Constructor_Should_Allow_Null_Notes()
    {
        // Arrange & Act
        var orderItem = fixture.CreateOrderItem(notes: null);

        // Assert
        orderItem.Notes.ShouldBeNull();
    }
}