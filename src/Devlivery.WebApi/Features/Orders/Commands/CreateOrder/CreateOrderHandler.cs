using Devlivery.WebApi.Features.Orders.Domain;
using Devlivery.WebApi.Shared.Database.Context;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.Orders.Commands.CreateOrder;

public sealed class CreateOrderHandler(ApplicationDbContext dbContext)
{
    public async Task<Result<CreateOrderResponse>> HandleAsync(
        CreateOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<PaymentMethod>(command.PaymentMethod, ignoreCase: true, out var paymentMethod))
            return Result.Fail("Método de pagamento inválido");

        var productIds = command.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
        {
            return Result.Fail("Um ou mais produtos não foram encontrados");
        }

        var itemsSubtotal = command.Items.Sum(item =>
        {
            var product = products.First(p => p.Id == item.ProductId);
            return product.Price * item.Quantity;
        });
        var total = itemsSubtotal + command.DeliveryFee;

        var now = DateTime.UtcNow;
        var order = new Order
        {
            Id = Guid.CreateVersion7(),
            CustomerName = command.CustomerName,
            CustomerPhone = command.CustomerPhone,
            DeliveryAddress = command.DeliveryAddress,
            Status = "pending",
            PaymentMethod = paymentMethod,
            Total = total,
            DeliveryFee = command.DeliveryFee,
            CreatedAt = now,
            UpdatedAt = now
        };

        var orderItems = command.Items.Select(item => new OrderItem
        {
            Id = Guid.CreateVersion7(),
            OrderId = order.Id,
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            Notes = item.Notes
        }).ToList();

        order.Items = orderItems;

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);


        var productsDict = products.ToDictionary(k => k.Id, v => v);
        var orderItemsWithProducts = orderItems.Select(oi => new OrderItemResponseDto(
            new ProductResponseDto(
                productsDict[oi.ProductId].Id,
                productsDict[oi.ProductId].Name,
                productsDict[oi.ProductId].Description,
                productsDict[oi.ProductId].Price,
                productsDict[oi.ProductId].Category,
                productsDict[oi.ProductId].CreatedAt,
                productsDict[oi.ProductId].UpdatedAt),
            oi.Quantity,
            oi.Notes)).ToArray();

        return new CreateOrderResponse(
            order.Id,
            orderItemsWithProducts,
            order.CustomerName,
            order.CustomerPhone,
            order.DeliveryAddress,
            order.PaymentMethod.ToString(),
            order.Status,
            order.Total,
            order.DeliveryFee,
            order.CreatedAt,
            order.UpdatedAt);
    }
}