using System.Net;
using Devlivery.Shared.Persistence.Context;
using Devlivery.Tests.Common;
using Devlivery.Tests.Common.Builders;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Devlivery.Tests.Features.Products.Commands.DeleteProduct;

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

        var (_, establishment, accessToken) = await Prepare();
        var product = new ProductBuilder()
            .WithEstablishmentId(establishment.Id)
            .Build();

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await DeleteAsync($"/api/products/{product.Id}", accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteProduct_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        await ResetDatabaseAsync();

        var (_, _, accessToken) = await Prepare();
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await DeleteAsync($"/api/products/{nonExistingId}", accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}