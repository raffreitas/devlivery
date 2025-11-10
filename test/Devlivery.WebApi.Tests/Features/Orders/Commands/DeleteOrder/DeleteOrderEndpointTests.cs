using System.Net;
using Devlivery.WebApi.Tests.Common;
using Devlivery.WebApi.Tests.Common.Builders;
using Shouldly;

namespace Devlivery.WebApi.Tests.Features.Orders.Commands.DeleteOrder;

[Trait("Category", "Integration Tests")]
public sealed class DeleteOrderEndpointTests(CustomWebApplicationFactory factory)
    : WebApiBaseFixture(factory), IAsyncLifetime
{
    [Fact]
    public async Task DeleteOrder_WithExistingOrder_ReturnsNoContent()
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
        var response = await DeleteAsync($"/api/orders/{order.Id}", token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteOrder_WithNonExistingId_ReturnsNotFound()
    {
        var token = await GetAccessTokenAsync();
        var nonExisting = Guid.NewGuid();

        var response = await DeleteAsync($"/api/orders/{nonExisting}", token);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await CleanUpDatabaseAsync();
}