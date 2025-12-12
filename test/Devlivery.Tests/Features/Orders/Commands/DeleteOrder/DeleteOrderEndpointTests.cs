using System.Net;
using Devlivery.Shared.Infrastructure.Persistence.Context;
using Devlivery.Tests.Common;
using Devlivery.Tests.Common.Builders;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Devlivery.Tests.Features.Orders.Commands.DeleteOrder;

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

        var (_, establishment, accessToken) = await Prepare();
        var product = new ProductBuilder()
            .WithEstablishmentId(establishment.Id)
            .Build();
        var orderItem = new OrderItemBuilder()
            .WithEstablishmentId(establishment.Id)
            .WithProduct(product)
            .Build();
        var order = new OrderBuilder()
            .WithEstablishmentId(establishment.Id)
            .WithItems(orderItem)
            .WithDeliveryFee(0m)
            .Build();

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.AddRangeAsync(product, order);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await DeleteAsync($"/api/orders/{order.Id}", accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteOrder_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        await ResetDatabaseAsync();

        var (_, _, accessToken) = await Prepare();
        var nonExisting = Guid.NewGuid();

        // Act
        var response = await DeleteAsync($"/api/orders/{nonExisting}", accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}