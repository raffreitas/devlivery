using Devlivery.WebApi.Shared.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.Orders;

public static class GetOrderById
{
    public record OrderItemDto(
        ProductDto Product,
        int Quantity,
        string? Notes);

    public record ProductDto(
        Guid Id,
        string Name,
        string Description,
        decimal Price,
        string Category,
        bool Available,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    public record Response(
        Guid Id,
        List<OrderItemDto> Items,
        string CustomerName,
        string CustomerPhone,
        string DeliveryAddress,
        string Status,
        decimal Total,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    public static async Task<IResult> Handle(Guid id, ApplicationDbContext db)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
        {
            return Results.NotFound();
        }

        var response = new Response(
            order.Id,
            order.Items.Select(i => new OrderItemDto(
                new ProductDto(
                    i.Product.Id,
                    i.Product.Name,
                    i.Product.Description,
                    i.Product.Price,
                    i.Product.Category,
                    i.Product.Available,
                    i.Product.CreatedAt,
                    i.Product.UpdatedAt),
                i.Quantity,
                i.Notes)).ToList(),
            order.CustomerName,
            order.CustomerPhone,
            order.DeliveryAddress,
            order.Status,
            order.Total,
            order.CreatedAt,
            order.UpdatedAt);

        return Results.Ok(response);
    }
}
