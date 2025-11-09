using System.Net;
using System.Text.Json;
using Devlivery.WebApi.Tests.Setup;
using Shouldly;

namespace Devlivery.WebApi.Tests.Features.Orders.Commands.CreateOrder;

[Trait("Category", "Integration Tests")]
public sealed class CreateOrderEndpointTests(CustomWebApplicationFactory factory) : WebApiBaseFixture(factory), IAsyncLifetime
{
    [Fact]
    public async Task CreateOrder_WithValidData_ReturnsCreatedAndOrder()
    {
        // Arrange
        var token = await GetAccessTokenAsync();

        // create a product to reference in order item
        var productCmd = new { Name = Faker.Commerce.ProductName(), Description = Faker.Commerce.ProductDescription(), Price = 10.0m, Category = Faker.Commerce.Categories(1)[0], Available = true };
        var prodResp = await PostAsync("/api/products", productCmd, token);
        prodResp.StatusCode.ShouldBe(HttpStatusCode.Created);
        await using var prodBody = await prodResp.Content.ReadAsStreamAsync();
        var prodData = await JsonDocument.ParseAsync(prodBody);
        var productId = prodData.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        var orderCommand = new
        {
            Items = new[] { new { ProductId = productId, Quantity = 2, Notes = "No onions" } },
            CustomerName = Faker.Name.FullName(),
            CustomerPhone = Faker.Phone.PhoneNumber(),
            DeliveryAddress = Faker.Address.FullAddress(),
            PaymentMethod = "cash",
            DeliveryFee = 5.0m
        };

        // Act
        var response = await PostAsync("/api/orders", orderCommand, token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        var data = responseData.RootElement.GetProperty("data");
        data.GetProperty("id").GetGuid().ShouldNotBe(Guid.Empty);
        data.GetProperty("customerName").GetString().ShouldBe(orderCommand.CustomerName);
        data.GetProperty("items").EnumerateArray().First().GetProperty("quantity").GetInt32().ShouldBe(2);
    }

    [Fact]
    public async Task CreateOrder_WithInvalidData_ReturnsValidationProblem()
    {
        // Arrange
        var token = await GetAccessTokenAsync();

        // invalid: empty items
        var orderCommand = new { Items = Array.Empty<object>(), CustomerName = "", CustomerPhone = "", DeliveryAddress = "", PaymentMethod = "", DeliveryFee = -1m };

        // Act
        var response = await PostAsync("/api/orders", orderCommand, token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        responseData.RootElement.TryGetProperty("errors", out _).ShouldBeTrue();
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await CleanUpDatabaseAsync();
}
