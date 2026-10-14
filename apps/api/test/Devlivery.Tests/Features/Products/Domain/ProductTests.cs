using Devlivery.Domain.Aggregates.Products;
using Devlivery.Tests.Common.Builders;

using Shouldly;

namespace Devlivery.Tests.Features.Products.Domain;

[Trait("Category", "Unit Tests")]
public sealed class ProductTests
{
    [Fact]
    public void GivenValidData_WhenCreatingProduct_ThenInitializesProperties()
    {
        // Arrange
        var establishmentId = Guid.NewGuid();

        // Act
        var product = new Product("Pizza Calabresa", "Molho, calabresa e cebola", 49.90m, "Pizzas", true,
            establishmentId);

        // Assert
        product.Name.ShouldBe("Pizza Calabresa");
        product.Description.ShouldBe("Molho, calabresa e cebola");
        product.Price.ShouldBe(49.90m);
        product.Category.ShouldBe("Pizzas");
        product.Available.ShouldBeTrue();
        product.EstablishmentId.ShouldBe(establishmentId);
        product.CreatedAt.ShouldNotBe(default);
        product.UpdatedAt.ShouldBe(product.CreatedAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void GivenProduct_WhenUpdatingAllFields_ThenReplacesValuesAndRefreshesUpdatedAt()
    {
        // Arrange
        var product = new ProductBuilder().Build();
        var beforeUpdate = DateTime.UtcNow;

        // Act
        product.Update("Pizza Portuguesa", "Presunto, ovo e cebola", 54.90m, "Pizzas Especiais");

        // Assert
        product.Name.ShouldBe("Pizza Portuguesa");
        product.Description.ShouldBe("Presunto, ovo e cebola");
        product.Price.ShouldBe(54.90m);
        product.Category.ShouldBe("Pizzas Especiais");
        product.UpdatedAt.ShouldBeGreaterThanOrEqualTo(beforeUpdate);
    }

    [Fact]
    public void GivenProduct_WhenUpdatingWithNullFields_ThenKeepsOriginalValues()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithName("Pizza Calabresa")
            .WithDescription("Molho, calabresa e cebola")
            .WithPrice(49.90m)
            .WithCategory("Pizzas")
            .Build();

        // Act
        product.Update();

        // Assert
        product.Name.ShouldBe("Pizza Calabresa");
        product.Description.ShouldBe("Molho, calabresa e cebola");
        product.Price.ShouldBe(49.90m);
        product.Category.ShouldBe("Pizzas");
    }

    [Fact]
    public void GivenUnavailableProduct_WhenSettingAsAvailable_ThenProductIsAvailable()
    {
        // Arrange
        var product = new ProductBuilder().AsUnavailable().Build();
        var beforeUpdate = DateTime.UtcNow;

        // Act
        product.SetAsAvailable();

        // Assert
        product.Available.ShouldBeTrue();
        product.UpdatedAt.ShouldBeGreaterThanOrEqualTo(beforeUpdate);
    }

    [Fact]
    public void GivenAvailableProduct_WhenSettingAsUnavailable_ThenProductIsUnavailable()
    {
        // Arrange
        var product = new ProductBuilder().Build();
        var beforeUpdate = DateTime.UtcNow;

        // Act
        product.SetAsUnavailable();

        // Assert
        product.Available.ShouldBeFalse();
        product.UpdatedAt.ShouldBeGreaterThanOrEqualTo(beforeUpdate);
    }
}