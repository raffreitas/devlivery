using System.Net;
using System.Text.Json;
using Devlivery.WebApi.Shared.Database.Context;
using Devlivery.WebApi.Tests.Common;
using Devlivery.WebApi.Tests.Common.Builders;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Devlivery.WebApi.Tests.Features.Products.Queries.GetProductById;

[Collection("Products Tests")]
[Trait("Category", "Integration Tests")]
public sealed class GetProductByIdEndpointTests(ProductsWebApplicationFactory factory)
    : WebApiBaseFixture<ProductsWebApplicationFactory>(factory)
{
    [Fact]
    public async Task GetProductById_ReturnsProduct()
    {
        // Arrange
        await ResetDatabaseAsync();

        var token = await GetAccessTokenAsync();

        var existingProduct = new ProductBuilder().Build();

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Products.AddAsync(existingProduct);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await GetAsync($"/api/products/{existingProduct.Id}", token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var data = await JsonDocument.ParseAsync(responseBody);
        var product = data.RootElement.GetProperty("data");
        product.GetProperty("id").GetGuid().ShouldBe(existingProduct.Id);
    }
}