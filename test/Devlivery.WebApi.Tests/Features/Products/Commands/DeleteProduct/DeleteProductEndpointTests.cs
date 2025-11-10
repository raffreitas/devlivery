using System.Net;
using Devlivery.WebApi.Tests.Common;
using Devlivery.WebApi.Tests.Common.Builders;
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
        var product = new ProductBuilder().Build();
        await AppDbContext.Products.AddAsync(product);
        await AppDbContext.SaveChangesAsync();

        // Act
        var response = await DeleteAsync($"/api/products/{product.Id}", token);

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