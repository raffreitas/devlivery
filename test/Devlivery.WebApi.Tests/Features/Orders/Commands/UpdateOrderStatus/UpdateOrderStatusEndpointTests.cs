using System.Net;
using System.Text.Json;
using Devlivery.WebApi.Tests.Setup;
using Shouldly;

namespace Devlivery.WebApi.Tests.Features.Orders.Commands.UpdateOrderStatus;

[Trait("Category", "Integration Tests")]
public sealed class UpdateOrderStatusEndpointTests(CustomWebApplicationFactory factory) : WebApiBaseFixture(factory), IAsyncLifetime
{
    [Fact]
    public async Task UpdateOrderStatus_WithValidData_ReturnsOkAndUpdatedStatus()
    {
        // Arrange
        var token = await GetAccessTokenAsync();

        var productCmd = new { Name = Faker.Commerce.ProductName(), Description = Faker.Commerce.ProductDescription(), Price = 15.0m, Category = Faker.Commerce.Categories(1)[0], Available = true };
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
            DeliveryFee = 0m
        };

        var createResp = await PostAsync("/api/orders", orderCommand, token);
        createResp.StatusCode.ShouldBe(HttpStatusCode.Created);
        await using var createBody = await createResp.Content.ReadAsStreamAsync();
        var created = await JsonDocument.ParseAsync(createBody);
        var id = created.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        var patch = new { Status = "preparing" };

        // Act
        var response = await PatchAsync($"/api/orders/{id}/status", patch, token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var data = await JsonDocument.ParseAsync(responseBody);
        data.RootElement.GetProperty("data").GetProperty("status").GetString().ShouldBe("preparing");
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
