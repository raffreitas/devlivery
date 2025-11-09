using System.Net;
using System.Text.Json;
using Devlivery.WebApi.Features.Products.Commands.CreateProduct;
using Devlivery.WebApi.Tests.Setup;
using Shouldly;

namespace Devlivery.WebApi.Tests.Features.Products.Commands.CreateProduct;

[Trait("Category", "Integration Tests")]
public sealed class CreateProductEndpointTests(CustomWebApplicationFactory factory)
    : WebApiBaseFixture(factory), IAsyncLifetime
{
    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsCreatedAndProduct()
    {
        // Arrange
        var accessToken = await GetAccessTokenAsync();
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
        responseData.ShouldNotBeNull();
        var apiResponse = responseData.RootElement.GetProperty("data");
        apiResponse.GetProperty("id").GetGuid().ShouldNotBe(Guid.Empty);
        apiResponse.GetProperty("name").GetString().ShouldBe(name);
        apiResponse.GetProperty("description").GetString().ShouldBe(description);
        apiResponse.GetProperty("price").GetDecimal().ShouldBe(price);
        apiResponse.GetProperty("category").GetString().ShouldBe(category);
        apiResponse.GetProperty("available").GetBoolean().ShouldBe(available);
    }

    [Fact]
    public async Task CreateProduct_WithInvalidData_ReturnsValidationProblem()
    {
        // Arrange - invalid: empty name and non-positive price
        var accessToken = await GetAccessTokenAsync();
        var command = new CreateProductCommand("", "", 0m, "", false);

        // Act
        var response = await PostAsync("/api/products", command, accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        responseData.RootElement.TryGetProperty("errors", out var errors).ShouldBeTrue();
        errors.ValueKind.ShouldBe(JsonValueKind.Object);
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await CleanUpDatabaseAsync();
}