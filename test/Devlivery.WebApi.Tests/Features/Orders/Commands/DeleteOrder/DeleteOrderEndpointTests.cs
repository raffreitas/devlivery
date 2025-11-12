using System.Net;
using Devlivery.WebApi.Shared.Database.Context;
using Devlivery.WebApi.Tests.Common;
using Devlivery.WebApi.Tests.Common.Builders;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Devlivery.WebApi.Tests.Features.Orders.Commands.DeleteOrder;

[Collection("Orders Tests")]
[Trait("Category", "Integration Tests")]
public sealed class DeleteOrderEndpointTests(OrdersWebApplicationFactory factory)
    : WebApiBaseFixture<OrdersWebApplicationFactory>(factory)
{
    [Fact]
    public async Task DeleteOrder_WithExistingOrder_ReturnsNoContent()
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
        await dbContext.AddRangeAsync(product, order);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await DeleteAsync($"/api/orders/{order.Id}", token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteOrder_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        await ResetDatabaseAsync();

        var establishmentId = Guid.NewGuid();
        var token = await GetAccessTokenAsync(establishmentId: establishmentId);
        var nonExisting = Guid.NewGuid();

        // Act
        var response = await DeleteAsync($"/api/orders/{nonExisting}", token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}