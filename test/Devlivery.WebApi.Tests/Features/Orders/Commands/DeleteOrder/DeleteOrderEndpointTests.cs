using System.Net;
using System.Text.Json;
using Devlivery.WebApi.Tests.Setup;
using Shouldly;

namespace Devlivery.WebApi.Tests.Features.Orders.Commands.DeleteOrder;

[Trait("Category", "Integration Tests")]
public sealed class DeleteOrderEndpointTests(CustomWebApplicationFactory factory) : WebApiBaseFixture(factory), IAsyncLifetime
{
    [Fact]
    public async Task DeleteOrder_WithExistingOrder_ReturnsNoContent()
    {
        var token = await GetAccessTokenAsync();

        var productCmd = new { Name = Faker.Commerce.ProductName(), Description = Faker.Commerce.ProductDescription(), Price = 9.0m, Category = Faker.Commerce.Categories(1)[0], Available = true };
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

        // Act
        var response = await DeleteAsync($"/api/orders/{id}", token);

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
