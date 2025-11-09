using System.Net;
using System.Text.Json;
using Devlivery.WebApi.Tests.Setup;
using Shouldly;

namespace Devlivery.WebApi.Tests.Features.Products.Commands.DeleteProduct;

[Trait("Category", "Integration Tests")]
public sealed class DeleteProductEndpointTests(CustomWebApplicationFactory factory)
    : WebApiBaseFixture(factory), IAsyncLifetime
{
    [Fact]
    public async Task DeleteProduct_WithExistingProduct_ReturnsNoContent()
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

        // Act
        var response = await DeleteAsync($"/api/products/{id}", token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteProduct_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await DeleteAsync($"/api/products/{nonExistingId}", token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await CleanUpDatabaseAsync();
}