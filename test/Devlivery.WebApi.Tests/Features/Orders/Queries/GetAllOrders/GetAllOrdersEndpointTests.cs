using System.Net;
using System.Text.Json;
using Devlivery.WebApi.Shared.Database.Context;
using Devlivery.WebApi.Tests.Common;
using Devlivery.WebApi.Tests.Common.Builders;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Devlivery.WebApi.Tests.Features.Orders.Queries.GetAllOrders;

[Collection("Orders Tests")]
[Trait("Category", "Integration Tests")]
public sealed class GetAllOrdersEndpointTests(OrdersWebApplicationFactory factory)
    : WebApiBaseFixture<OrdersWebApplicationFactory>(factory)
{
    [Fact]
    public async Task GetAllOrders_ReturnsListOfOrders()
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
        await dbContext.Products.AddAsync(product);
        await dbContext.Orders.AddAsync(order);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await GetAsync("/api/orders", accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var data = await JsonDocument.ParseAsync(responseBody);
        var list = data.RootElement.GetProperty("data").EnumerateArray().ToList();
        list.Count.ShouldBeGreaterThanOrEqualTo(1);
    }
}