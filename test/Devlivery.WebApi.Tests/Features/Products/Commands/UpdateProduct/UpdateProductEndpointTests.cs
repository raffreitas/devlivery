using System.Net;
using System.Text.Json;
using Devlivery.WebApi.Tests.Setup;
using Shouldly;

namespace Devlivery.WebApi.Tests.Features.Products.Commands.UpdateProduct;

[Trait("Category", "Integration Tests")]
public sealed class UpdateProductEndpointTests(CustomWebApplicationFactory factory)
    : WebApiBaseFixture(factory), IAsyncLifetime
{
    [Fact]
    public async Task UpdateProduct_WithValidData_ReturnsOkAndUpdatedProduct()
    {
        // Arrange
        var token = await GetAccessTokenAsync();

        var createCommand = new
        {
            Name = Faker.Commerce.ProductName(),
            Description = Faker.Commerce.ProductDescription(),
            Price = Faker.Random.Decimal(1.0m, 500m),
            Category = Faker.Commerce.Categories(1)[0],
            Available = true
        };
        var createResponse = await PostAsync("/api/products", createCommand, token);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        await using var createBody = await createResponse.Content.ReadAsStreamAsync();
        var created = await JsonDocument.ParseAsync(createBody);
        var id = created.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        var updateRequest = new
        {
            Name = "Updated " + Faker.Commerce.ProductName(),
            Description = "Updated " + Faker.Commerce.ProductDescription(),
            Price = Faker.Random.Decimal(501m, 999m),
            Category = Faker.Commerce.Categories(1)[0],
            Available = false
        };

        // Act
        var response = await PutAsync($"/api/products/{id}", updateRequest, token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var data = await JsonDocument.ParseAsync(responseBody);
        var product = data.RootElement.GetProperty("data");
        product.GetProperty("id").GetGuid().ShouldBe(id);
        product.GetProperty("name").GetString().ShouldBe(updateRequest.Name);
    }

    [Fact]
    public async Task UpdateProduct_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        var nonExistingId = Guid.NewGuid();

        var updateRequest = new
        {
            Name = "Does not matter",
            Description = "Does not matter",
            Price = 10.0m,
            Category = "None",
            Available = false
        };

        // Act
        var response = await PutAsync($"/api/products/{nonExistingId}", updateRequest, token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await CleanUpDatabaseAsync();
}