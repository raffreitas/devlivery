using System.Net;
using System.Text.Json;
using Devlivery.WebApi.Features.Orders.Domain;
using Devlivery.WebApi.Shared.Database.Context;
using Devlivery.WebApi.Tests.Common;
using Devlivery.WebApi.Tests.Common.Builders;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Devlivery.WebApi.Tests.Features.Orders.Commands.CreateOrder;

[Collection("Orders Tests")]
[Trait("Category", "Integration Tests")]
public sealed class CreateOrderEndpointTests(OrdersWebApplicationFactory factory)
    : WebApiBaseFixture<OrdersWebApplicationFactory>(factory)
{
    [Fact]
    public async Task CreateOrder_WithValidData_ReturnsCreatedAndOrder()
    {
        // Arrange
        await ResetDatabaseAsync();
        var establishmentId = Guid.NewGuid();
        var token = await GetAccessTokenAsync(establishmentId: establishmentId);
        var product = new ProductBuilder().Build();

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var request = new
        {
            Items = new[]
            {
                new
                {
                    ProductId = product.Id,
                    Quantity = Faker.Random.Number(1, 10),
                    Notes = Faker.Lorem.Sentence(),
                }
            },
            CustomerName = Faker.Name.FullName(),
            CustomerPhone = Faker.Phone.PhoneNumber(),
            DeliveryAddress = Faker.Address.FullAddress(),
            PaymentMethod = Faker.PickRandom<PaymentMethod>().ToString(),
            DeliveryFee = Faker.Random.Decimal(0, 100)
        };

        // Act
        var response = await PostAsync("/api/orders", request, token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        var data = responseData.RootElement.GetProperty("data");
        data.GetProperty("orderId").GetGuid().ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateOrder_WithInvalidData_ReturnsValidationProblem()
    {
        // Arrange
        await ResetDatabaseAsync();

        var establishmentId = Guid.NewGuid();
        var user = await CreateUserAsync(establishmentId: establishmentId);
        var token = await GetAccessTokenAsync(user);

        // invalid: empty items
        var orderCommand = new
        {
            Items = Array.Empty<object>(),
            CustomerName = "",
            CustomerPhone = "",
            DeliveryAddress = "",
            PaymentMethod = "",
            DeliveryFee = -1m
        };

        // Act
        var response = await PostAsync("/api/orders", orderCommand, token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        responseData.RootElement.TryGetProperty("errors", out _).ShouldBeTrue();
    }
}