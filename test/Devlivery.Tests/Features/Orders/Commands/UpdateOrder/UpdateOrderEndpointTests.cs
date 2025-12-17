using System.Net;

using Devlivery.Features.Orders.Domain.Enums;
using Devlivery.Shared.Infrastructure.Persistence.Context;
using Devlivery.Tests.Common;
using Devlivery.Tests.Common.Builders;

using Microsoft.Extensions.DependencyInjection;

using Shouldly;

namespace Devlivery.Tests.Features.Orders.Commands.UpdateOrder;

[Collection("Orders Tests")]
[Trait("Category", "Integration Tests")]
public sealed class UpdateOrderEndpointTests(OrdersWebApplicationFactory factory)
    : WebApiBaseFixture<OrdersWebApplicationFactory>(factory)
{
    [Fact]
    public async Task UpdateOrder_WithValidData_ReturnsOkAndUpdatedOrder()
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

        var request = new
        {
            order.Id,
            Items = new[]
            {
                new
                {
                    ProductId = product.Id,
                    Quantity = 2,
                    Notes = "sem cebola"
                }
            },
            CustomerName = "Cliente Atualizado",
            CustomerPhone = "11999998888",
            DeliveryAddress = "Rua Nova, 123",
            PaymentMethod = nameof(PaymentMethod.Cash),
            DeliveryFee = 5.0m
        };

        // Act
        var response = await PutAsync($"/api/orders/{order.Id}", request, accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateOrder_WhenOrderIsCancelledOrDelivered_ReturnsUnprocessableEntity()
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

        // mark as canceled
        order.UpdateStatus(OrderStatus.Canceled);

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Products.AddAsync(product);
        await dbContext.Orders.AddAsync(order);
        await dbContext.SaveChangesAsync();

        var request = new
        {
            order.Id,
            Items = new[]
            {
                new { ProductId = product.Id, Quantity = 1, Notes = "" }
            },
            CustomerName = "Teste",
            CustomerPhone = "",
            DeliveryAddress = "Endereço",
            PaymentMethod = nameof(PaymentMethod.Cash),
            DeliveryFee = 0m
        };

        // Act
        var response = await PutAsync($"/api/orders/{order.Id}", request, accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateOrder_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        await ResetDatabaseAsync();
        var (_, _, accessToken) = await Prepare();

        var product = new ProductBuilder()
            .WithEstablishmentId(Guid.NewGuid())
            .Build();

        var requestId = Guid.NewGuid();

        var request = new
        {
            Id = requestId,
            Items = new[]
            {
                new { ProductId = product.Id, Quantity = 1, Notes = "" }
            },
            CustomerName = "Teste",
            CustomerPhone = "",
            DeliveryAddress = "Endereço",
            PaymentMethod = nameof(PaymentMethod.Cash),
            DeliveryFee = 0m
        };

        // Act
        var response = await PutAsync($"/api/orders/{requestId}", request, accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}