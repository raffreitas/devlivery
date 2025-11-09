using System.Net;
using System.Text.Json;
using Devlivery.WebApi.Tests.Setup;
using Shouldly;

namespace Devlivery.WebApi.Tests.Features.Products.Queries.GetProductById;

[Trait("Category", "Integration Tests")]
public sealed class GetProductByIdEndpointTests(CustomWebApplicationFactory factory)
    : WebApiBaseFixture(factory), IAsyncLifetime
{
    [Fact]
    public async Task GetProductById_ReturnsProduct()
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
        var response = await GetAsync($"/api/products/{id}", token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var data = await JsonDocument.ParseAsync(responseBody);
        var product = data.RootElement.GetProperty("data");
        product.GetProperty("id").GetGuid().ShouldBe(id);
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await CleanUpDatabaseAsync();
}