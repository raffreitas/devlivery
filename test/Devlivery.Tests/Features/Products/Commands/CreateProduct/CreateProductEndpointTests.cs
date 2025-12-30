using System.Net;
using System.Text.Json;

using Devlivery.Features.Products.Commands.CreateProduct;
using Devlivery.Tests.Common;

using Shouldly;

namespace Devlivery.Tests.Features.Products.Commands.CreateProduct;

[Collection("Products Tests")]
[Trait("Category", "Integration Tests")]
public sealed class CreateProductEndpointTests(ProductsWebApplicationFactory factory)
    : WebApiBaseFixture<ProductsWebApplicationFactory>(factory)
{
    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsCreatedAndProduct()
    {
        // Arrange
        await ResetDatabaseAsync();

        var (_, _, accessToken) = await Prepare();
        var name = Faker.Commerce.ProductName();
        var description = Faker.Commerce.ProductDescription();
        var price = Faker.Random.Decimal(1.0m, 999.99m);
        var category = Faker.Commerce.Categories(1)[0];
        const bool available = true;

        var command = new CreateProductCommand(name, description, price, category, available);

        // Act
        var response = await PostAsync("/api/products", command, accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        responseData.RootElement.TryGetProperty("data", out var data).ShouldBeTrue();
        data.GetProperty("productId").GetGuid().ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateProduct_WithInvalidData_ReturnsValidationProblem()
    {
        // Arrange
        await ResetDatabaseAsync();

        var (_, _, accessToken) = await Prepare();
        var command = new CreateProductCommand("", "", 0m, "", false);

        // Act
        var response = await PostAsync("/api/products", command, accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        responseData.RootElement.TryGetProperty("success", out var success).ShouldBeTrue();
        success.GetBoolean().ShouldBeFalse();
        responseData.RootElement.TryGetProperty("errors", out var errors).ShouldBeTrue();
        errors.ValueKind.ShouldBe(JsonValueKind.Array);
        errors.GetArrayLength().ShouldBeGreaterThan(0);
    }
}