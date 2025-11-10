using System.Net;
using System.Text.Json;
using Devlivery.WebApi.Tests.Common;
using Devlivery.WebApi.Tests.Common.Builders;
using Shouldly;

namespace Devlivery.WebApi.Tests.Features.Orders.Queries.GetOrderById;

[Trait("Category", "Integration Tests")]
public sealed class GetOrderByIdEndpointTests(CustomWebApplicationFactory factory)
    : WebApiBaseFixture(factory), IAsyncLifetime
{
    [Fact]
    public async Task GetOrderById_ReturnsOrder()
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
        var response = await GetAsync($"/api/orders/{order.Id}", token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var data = await JsonDocument.ParseAsync(responseBody);
        data.RootElement.GetProperty("data").GetProperty("id").GetGuid().ShouldBe(order.Id);
    }

    [Fact]
    public async Task GetOrderById_WithNonExistingId_ReturnsNotFound()
    {
        var token = await GetAccessTokenAsync();
        var nonExisting = Guid.NewGuid();

        var response = await GetAsync($"/api/orders/{nonExisting}", token);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await CleanUpDatabaseAsync();
}