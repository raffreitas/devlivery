using Devlivery.Domain.Aggregates.Products;

using Shouldly;

namespace Devlivery.Tests.Features.Products.Domain;

[Collection("Products Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class ProductTests(ProductsUnitTestFixture fixture)
{
    [Fact]
    public void Constructor_Should_Create_Product_With_Correct_Properties()
    {
        // Arrange
        const string name = "Produto Teste";
        const string description = "Descrição do produto";
        const decimal price = 99.99m;
        const string category = "Eletrônicos";
        const bool available = true;
        var establishmentId = Guid.NewGuid();

        // Act
        var product = new Product(name, description, price, category, available, establishmentId);

        // Assert
        product.Name.ShouldBe(name);
        product.Description.ShouldBe(description);
        product.Price.ShouldBe(price);
        product.Category.ShouldBe(category);
        product.Available.ShouldBe(available);
        product.EstablishmentId.ShouldBe(establishmentId);
        product.CreatedAt.ShouldNotBe(default);
        product.UpdatedAt.ShouldNotBe(default);
    }

    [Fact]
    public async Task Update_Should_Update_All_Properties()
    {
        // Arrange
        var product = fixture.CreateProduct(
            name: "Nome Original",
            description: "Descrição Original",
            price: 50.00m,
            category: "Categoria Original"
        );

        var originalUpdatedAt = product.UpdatedAt;
        await Task.Delay(10);

        const string newName = "Nome Atualizado";
        const string newDescription = "Descrição Atualizada";
        const decimal newPrice = 100.00m;
        const string newCategory = "Nova Categoria";

        // Act
        product.Update(newName, newDescription, newPrice, newCategory);

        // Assert
        product.Name.ShouldBe(newName);
        product.Description.ShouldBe(newDescription);
        product.Price.ShouldBe(newPrice);
        product.Category.ShouldBe(newCategory);
        product.UpdatedAt.ShouldBeGreaterThan(originalUpdatedAt);
    }

    [Fact]
    public void Update_Should_Keep_Original_Name_When_Null()
    {
        // Arrange
        const string originalName = "Nome Original";
        var product = fixture.CreateProduct(name: originalName);

        // Act
        product.Update(name: null, description: "Nova Descrição");

        // Assert
        product.Name.ShouldBe(originalName);
    }

    [Fact]
    public void Update_Should_Keep_Original_Description_When_Null()
    {
        // Arrange
        const string originalDescription = "Descrição Original";
        var product = fixture.CreateProduct(description: originalDescription);

        // Act
        product.Update(name: "Novo Nome", description: null);

        // Assert
        product.Description.ShouldBe(originalDescription);
    }

    [Fact]
    public void Update_Should_Keep_Original_Price_When_Null()
    {
        // Arrange
        const decimal originalPrice = 99.99m;
        var product = fixture.CreateProduct(price: originalPrice);

        // Act
        product.Update(name: "Novo Nome", price: null);

        // Assert
        product.Price.ShouldBe(originalPrice);
    }

    [Fact]
    public void Update_Should_Keep_Original_Category_When_Null()
    {
        // Arrange
        const string originalCategory = "Categoria Original";
        var product = fixture.CreateProduct(category: originalCategory);

        // Act
        product.Update(name: "Novo Nome", category: null);

        // Assert
        product.Category.ShouldBe(originalCategory);
    }

    [Fact]
    public async Task Update_Should_Update_UpdatedAt_Timestamp()
    {
        // Arrange
        var product = fixture.CreateProduct();
        var originalUpdatedAt = product.UpdatedAt;
        await Task.Delay(10);

        // Act
        product.Update(name: "Novo Nome");

        // Assert
        product.UpdatedAt.ShouldBeGreaterThan(originalUpdatedAt);
    }

    [Fact]
    public void SetAsAvailable_Should_Set_Available_To_True()
    {
        // Arrange
        var product = fixture.CreateProduct(available: false);

        // Act
        product.SetAsAvailable();

        // Assert
        product.Available.ShouldBeTrue();
    }

    [Fact]
    public async Task SetAsAvailable_Should_Update_UpdatedAt_Timestamp()
    {
        // Arrange
        var product = fixture.CreateProduct(available: false);
        var originalUpdatedAt = product.UpdatedAt;
        await Task.Delay(10);

        // Act
        product.SetAsAvailable();

        // Assert
        product.UpdatedAt.ShouldBeGreaterThan(originalUpdatedAt);
    }

    [Fact]
    public void SetAsUnavailable_Should_Set_Available_To_False()
    {
        // Arrange
        var product = fixture.CreateProduct(available: true);

        // Act
        product.SetAsUnavailable();

        // Assert
        product.Available.ShouldBeFalse();
    }

    [Fact]
    public async Task SetAsUnavailable_Should_Update_UpdatedAt_Timestamp()
    {
        // Arrange
        var product = fixture.CreateProduct(available: true);
        var originalUpdatedAt = product.UpdatedAt;
        await Task.Delay(10);

        // Act
        product.SetAsUnavailable();

        // Assert
        product.UpdatedAt.ShouldBeGreaterThan(originalUpdatedAt);
    }

    [Fact]
    public void SetAsAvailable_Should_Work_When_Already_Available()
    {
        // Arrange
        var product = fixture.CreateProduct(available: true);

        // Act
        product.SetAsAvailable();

        // Assert
        product.Available.ShouldBeTrue();
    }

    [Fact]
    public void SetAsUnavailable_Should_Work_When_Already_Unavailable()
    {
        // Arrange
        var product = fixture.CreateProduct(available: false);

        // Act
        product.SetAsUnavailable();

        // Assert
        product.Available.ShouldBeFalse();
    }
}