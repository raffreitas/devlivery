using System.Net;
using System.Text.Json;
using Devlivery.WebApi.Tests.Common;
using Devlivery.WebApi.Tests.Common.Builders;
using Shouldly;

namespace Devlivery.WebApi.Tests.Features.Orders.Queries.GetAllOrders;

[Trait("Category", "Integration Tests")]
public sealed class GetAllOrdersEndpointTests(CustomWebApplicationFactory factory)
    : WebApiBaseFixture(factory), IAsyncLifetime
{
    [Fact]
    public async Task GetAllOrders_ReturnsListOfOrders()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        var product = new ProductBuilder().Build();
        var orderItem = new OrderItemBuilder()
            .WithProductId(product.Id)
            .Build();
        var order = new OrderBuilder()
            .WithItems(orderItem)
            .WithDeliveryFee(0m)
            .WithTotal(orderItem.Quantity * product.Price)
            .Build();

        await AppDbContext.AddRangeAsync(product, order);
        await AppDbContext.SaveChangesAsync();

        // Act
        var response = await GetAsync("/api/orders", token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var data = await JsonDocument.ParseAsync(responseBody);
        var list = data.RootElement.GetProperty("data").EnumerateArray().ToList();
        list.Count.ShouldBeGreaterThanOrEqualTo(1);
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await CleanUpDatabaseAsync();
}