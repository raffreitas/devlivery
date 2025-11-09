using System.Net;
using System.Text.Json;
using Devlivery.WebApi.Tests.Setup;
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

        // create a product and an order
        var productCmd = new
        {
            Name = Faker.Commerce.ProductName(), Description = Faker.Commerce.ProductDescription(), Price = 8.0m,
            Category = Faker.Commerce.Categories(1)[0], Available = true
        };
        var prodResp = await PostAsync("/api/products", productCmd, token);
        prodResp.StatusCode.ShouldBe(HttpStatusCode.Created);
        await using var prodBody = await prodResp.Content.ReadAsStreamAsync();
        var prodData = await JsonDocument.ParseAsync(prodBody);
        var productId = prodData.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        var orderCommand = new
        {
            Items = new[] { new { ProductId = productId, Quantity = 1, Notes = "" } },
            CustomerName = Faker.Name.FullName(),
            CustomerPhone = Faker.Phone.PhoneNumber(),
            DeliveryAddress = Faker.Address.FullAddress(),
            PaymentMethod = "cash",
            DeliveryFee = 2.0m
        };

        var createResp = await PostAsync("/api/orders", orderCommand, token);
        createResp.StatusCode.ShouldBe(HttpStatusCode.Created);

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