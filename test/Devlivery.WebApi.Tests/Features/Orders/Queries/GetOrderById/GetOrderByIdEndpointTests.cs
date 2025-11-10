using System.Net;
using System.Text.Json;
using Devlivery.WebApi.Shared.Database.Context;
using Devlivery.WebApi.Tests.Common;
using Devlivery.WebApi.Tests.Common.Builders;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Devlivery.WebApi.Tests.Features.Orders.Queries.GetOrderById;

[Collection("Orders Tests")]
[Trait("Category", "Integration Tests")]
public sealed class GetOrderByIdEndpointTests(OrdersWebApplicationFactory factory)
    : WebApiBaseFixture<OrdersWebApplicationFactory>(factory)
{
    [Fact]
    public async Task GetOrderById_ReturnsOrder()
    {
        // Arrange
        await ResetDatabaseAsync();

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

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.AddRangeAsync(product, order);
        await dbContext.SaveChangesAsync();

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
        // Arrange
        await ResetDatabaseAsync();

        var token = await GetAccessTokenAsync();
        var nonExisting = Guid.NewGuid();

        // Act
        var response = await GetAsync($"/api/orders/{nonExisting}", token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}