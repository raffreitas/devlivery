using System.Net;
using Devlivery.WebApi.Shared.Database.Context;
using Devlivery.WebApi.Tests.Common;
using Devlivery.WebApi.Tests.Common.Builders;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Devlivery.WebApi.Tests.Features.Orders.Commands.UpdateOrderStatus;

[Collection("Orders Tests")]
[Trait("Category", "Integration Tests")]
public sealed class UpdateOrderStatusEndpointTests(OrdersWebApplicationFactory factory)
    : WebApiBaseFixture<OrdersWebApplicationFactory>(factory)
{
    [Fact]
    public async Task UpdateOrderStatus_WithValidData_ReturnsOkAndUpdatedStatus()
    {
        // Arrange
        await ResetDatabaseAsync();

        var establishmentId = Guid.NewGuid();
        var token = await GetAccessTokenAsync(establishmentId: establishmentId);
        var product = new ProductBuilder().Build();
        var orderItem = new OrderItemBuilder()
            .WithProduct(product)
            .Build();
        var order = new OrderBuilder()
            .WithItems(orderItem)
            .WithDeliveryFee(0m)
            .Build();

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Products.AddAsync(product);
        await dbContext.Orders.AddAsync(order);
        await dbContext.SaveChangesAsync();

        var patch = new { Status = "preparing" };

        // Act
        var response = await PatchAsync($"/api/orders/{order.Id}/status", patch, token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateOrderStatus_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        await ResetDatabaseAsync();

        var establishmentId = Guid.NewGuid();
        var token = await GetAccessTokenAsync(establishmentId: establishmentId);
        var nonExisting = Guid.NewGuid();
        var patch = new { Status = "ready" };

        // Act
        var response = await PatchAsync($"/api/orders/{nonExisting}/status", patch, token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}