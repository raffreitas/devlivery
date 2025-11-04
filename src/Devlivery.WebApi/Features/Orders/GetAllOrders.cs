using Devlivery.WebApi.Shared.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.Orders;

public static class GetAllOrders
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

    public static async Task<IResult> Handle(ApplicationDbContext db)
    {
        var orders = await db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var response = orders.Select(o => new Response(
            o.Id,
            o.Items.Select(i => new OrderItemDto(
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
            o.CustomerName,
            o.CustomerPhone,
            o.DeliveryAddress,
            o.Status,
            o.Total,
            o.CreatedAt,
            o.UpdatedAt)).ToList();

        return Results.Ok(response);
    }
}
