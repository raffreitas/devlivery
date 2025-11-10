using System.Net;
using System.Text.Json;
using Devlivery.WebApi.Tests.Common;
using Devlivery.WebApi.Tests.Common.Builders;
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

        var existingProduct = new ProductBuilder().Build();
        await AppDbContext.Products.AddAsync(existingProduct);
        await AppDbContext.SaveChangesAsync();

        // Act
        var response = await GetAsync($"/api/products/{existingProduct.Id}", token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var data = await JsonDocument.ParseAsync(responseBody);
        var product = data.RootElement.GetProperty("data");
        product.GetProperty("id").GetGuid().ShouldBe(existingProduct.Id);
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await CleanUpDatabaseAsync();
}