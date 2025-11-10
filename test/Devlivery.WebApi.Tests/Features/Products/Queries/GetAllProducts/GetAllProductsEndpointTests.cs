using System.Net;
using System.Text.Json;
using Devlivery.WebApi.Tests.Common;
using Devlivery.WebApi.Tests.Common.Builders;
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
        var product1 = new ProductBuilder().Build();
        var product2 = new ProductBuilder().Build();
        await AppDbContext.Products.AddRangeAsync(product1, product2);
        await AppDbContext.SaveChangesAsync();

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