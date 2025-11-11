using System.Net;
using System.Text.Json;
using Devlivery.WebApi.Shared.Database.Context;
using Devlivery.WebApi.Tests.Common;
using Devlivery.WebApi.Tests.Common.Builders;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Devlivery.WebApi.Tests.Features.Products.Queries.GetAllProducts;

[Collection("Products Tests")]
[Trait("Category", "Integration Tests")]
public sealed class GetAllProductsEndpointTests(ProductsWebApplicationFactory factory)
    : WebApiBaseFixture<ProductsWebApplicationFactory>(factory)
{
    [Fact]
    public async Task GetAllProducts_ReturnsListOfProducts()
    {
        // Arrange
        await ResetDatabaseAsync();

        var token = await GetAccessTokenAsync();
        var product1 = new ProductBuilder().Build();
        var product2 = new ProductBuilder().Build();

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Products.AddRangeAsync(product1, product2);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await GetAsync("/api/products", token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var data = await JsonDocument.ParseAsync(responseBody);
        var list = data.RootElement.GetProperty("data").EnumerateArray().ToList();
        list.Count.ShouldBeGreaterThanOrEqualTo(2);
    }
}