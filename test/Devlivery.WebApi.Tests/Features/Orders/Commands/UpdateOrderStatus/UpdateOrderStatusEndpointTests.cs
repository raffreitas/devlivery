using System.Net;
using Devlivery.WebApi.Tests.Common;
using Devlivery.WebApi.Tests.Common.Builders;
using Shouldly;

namespace Devlivery.WebApi.Tests.Features.Orders.Commands.UpdateOrderStatus;

[Trait("Category", "Integration Tests")]
public sealed class UpdateOrderStatusEndpointTests(CustomWebApplicationFactory factory)
    : WebApiBaseFixture(factory), IAsyncLifetime
{
    [Fact]
    public async Task UpdateOrderStatus_WithValidData_ReturnsOkAndUpdatedStatus()
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

        var patch = new { Status = "preparing" };

        // Act
        var response = await PatchAsync($"/api/orders/{order.Id}/status", patch, token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateOrderStatus_WithNonExistingId_ReturnsNotFound()
    {
        var token = await GetAccessTokenAsync();
        var nonExisting = Guid.NewGuid();
        var patch = new { Status = "ready" };

        var response = await PatchAsync($"/api/orders/{nonExisting}/status", patch, token);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await CleanUpDatabaseAsync();
}