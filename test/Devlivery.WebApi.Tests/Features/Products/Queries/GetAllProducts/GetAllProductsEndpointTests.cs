using System.Net;
using System.Text.Json;
using Devlivery.WebApi.Tests.Setup;
using Shouldly;

namespace Devlivery.WebApi.Tests.Features.Products.Queries.GetAllProducts;

[Trait("Category", "Integration Tests")]
public sealed class GetAllProductsEndpointTests(CustomWebApplicationFactory factory)
    : WebApiBaseFixture(factory), IAsyncLifetime
{
    [Fact]
    public async Task GetAllProducts_ReturnsListOfProducts()
    {
        // Arrange
        var token = await GetAccessTokenAsync();

        // create two products
        var command1 = new
        {
            Name = Faker.Commerce.ProductName(),
            Description = Faker.Commerce.ProductDescription(),
            Price = Faker.Random.Decimal(1.0m, 500m),
            Category = Faker.Commerce.Categories(1)[0],
            Available = true
        };
        var command2 = new
        {
            Name = Faker.Commerce.ProductName(),
            Description = Faker.Commerce.ProductDescription(),
            Price = Faker.Random.Decimal(1.0m, 500m),
            Category = Faker.Commerce.Categories(1)[0],
            Available = true
        };

        var r1 = await PostAsync("/api/products", command1, token);
        r1.StatusCode.ShouldBe(HttpStatusCode.Created);
        var r2 = await PostAsync("/api/products", command2, token);
        r2.StatusCode.ShouldBe(HttpStatusCode.Created);

        // Act
        var response = await GetAsync("/api/products", token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var data = await JsonDocument.ParseAsync(responseBody);
        var list = data.RootElement.GetProperty("data").EnumerateArray().ToList();
        list.Count.ShouldBeGreaterThanOrEqualTo(2);
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await CleanUpDatabaseAsync();
}