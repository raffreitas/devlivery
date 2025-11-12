using System.Net;
using Devlivery.WebApi.Shared.Database.Context;
using Devlivery.WebApi.Tests.Common;
using Devlivery.WebApi.Tests.Common.Builders;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Devlivery.WebApi.Tests.Features.Products.Commands.DeleteProduct;

[Collection("Products Tests")]
[Trait("Category", "Integration Tests")]
public sealed class DeleteProductEndpointTests(ProductsWebApplicationFactory factory)
    : WebApiBaseFixture<ProductsWebApplicationFactory>(factory)
{
    [Fact]
    public async Task DeleteProduct_WithExistingProduct_ReturnsNoContent()
    {
        // Arrange
        await ResetDatabaseAsync();

        var establishmentId = Guid.NewGuid();
        var token = await GetAccessTokenAsync(establishmentId: establishmentId);
        var product = new ProductBuilder().Build();

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await DeleteAsync($"/api/products/{product.Id}", token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteProduct_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        await ResetDatabaseAsync();

        var establishmentId = Guid.NewGuid();
        var token = await GetAccessTokenAsync(establishmentId: establishmentId);
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await DeleteAsync($"/api/products/{nonExistingId}", token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}