using System.Net;
using Devlivery.WebApi.Shared.Database.Context;
using Devlivery.WebApi.Tests.Common;
using Devlivery.WebApi.Tests.Common.Builders;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Devlivery.WebApi.Tests.Features.Products.Commands.UpdateProduct;

[Collection("Products Tests")]
[Trait("Category", "Integration Tests")]
public sealed class UpdateProductEndpointTests(ProductsWebApplicationFactory factory)
    : WebApiBaseFixture<ProductsWebApplicationFactory>(factory)
{
    [Fact]
    public async Task UpdateProduct_WithValidData_ReturnsOkAndUpdatedProduct()
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

        var updateRequest = new
        {
            Name = "Updated " + product.Name,
            Description = "Updated " + product.Description,
            Price = Faker.Random.Decimal(501m, 999m),
            Category = Faker.Commerce.Categories(1)[0],
            Available = false
        };

        // Act
        var response = await PutAsync($"/api/products/{product.Id}", updateRequest, accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateProduct_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        await ResetDatabaseAsync();

        var (_, establishment, accessToken) = await Prepare();
        var product = new ProductBuilder()
            .WithEstablishmentId(establishment.Id)
            .Build();
        var nonExistingId = Guid.NewGuid();

        var updateRequest = new
        {
            product.Name,
            product.Description,
            product.Price,
            product.Category,
            Available = false
        };

        // Act
        var response = await PutAsync($"/api/products/{nonExistingId}", updateRequest, accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}