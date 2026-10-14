using Devlivery.Domain.Aggregates.Orders.Entities;
using Devlivery.Domain.SeedWork;

using Shouldly;

namespace Devlivery.Tests.Features.Orders.Domain;

[Trait("Category", "Unit Tests")]
public sealed class OrderItemTests
{
    [Theory]
    [InlineData(1, 10.00, 10.00)]
    [InlineData(2, 15.50, 31.00)]
    [InlineData(3, 12.33, 36.99)]
    [InlineData(10, 7.99, 79.90)]
    public void GivenQuantityAndUnitPrice_WhenCreatingItem_ThenTotalPriceIsTheirProduct(
        int quantity, decimal unitPrice, decimal expectedTotal)
    {
        // Arrange
        var productId = Guid.NewGuid();

        // Act
        var item = new OrderItem(productId, Guid.NewGuid(), quantity, unitPrice, "Sem gelo");

        // Assert
        item.TotalPrice.ShouldBe(expectedTotal);
        item.ProductId.ShouldBe(productId);
        item.Notes.ShouldBe("Sem gelo");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GivenNonPositiveQuantity_WhenCreatingItem_ThenThrowsDomainException(int quantity)
    {
        // Arrange
        var productId = Guid.NewGuid();

        // Act
        var exception = Record.Exception(() => new OrderItem(productId, Guid.NewGuid(), quantity, 10m, null));

        // Assert
        exception.ShouldBeOfType<DomainException>().Message.ShouldBe("Quantidade deve ser maior que zero");
    }

    [Fact]
    public void GivenNegativeUnitPrice_WhenCreatingItem_ThenThrowsDomainException()
    {
        // Arrange
        var productId = Guid.NewGuid();

        // Act
        var exception = Record.Exception(() => new OrderItem(productId, Guid.NewGuid(), 1, -0.01m, null));

        // Assert
        exception.ShouldBeOfType<DomainException>().Message.ShouldBe("Preço unitário não pode ser negativo");
    }
}