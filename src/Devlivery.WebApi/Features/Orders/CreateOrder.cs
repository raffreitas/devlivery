using Devlivery.WebApi.Features.Orders.Domain;
using Devlivery.WebApi.Shared.Infrastructure.Database.Context;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.Orders;

public static class CreateOrder
{
    public record OrderItemRequest(Guid ProductId, int Quantity, string? Notes);

    public record Request(
        List<OrderItemRequest> Items,
        string CustomerName,
        string CustomerPhone,
        string DeliveryAddress);

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

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Items).NotEmpty();
            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(x => x.ProductId).NotEmpty();
                item.RuleFor(x => x.Quantity).GreaterThan(0);
            });
            RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.CustomerPhone).NotEmpty().MaximumLength(20);
            RuleFor(x => x.DeliveryAddress).NotEmpty().MaximumLength(500);
        }
    }

    public static async Task<IResult> Handle(
        Request request,
        ApplicationDbContext db,
        IValidator<Request> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        if (products.Count != productIds.Count)
        {
            return Results.BadRequest(new { message = "Um ou mais produtos não foram encontrados" });
        }

        var total = request.Items.Sum(item =>
        {
            var product = products.First(p => p.Id == item.ProductId);
            return product.Price * item.Quantity;
        });

        var now = DateTime.UtcNow;
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = request.CustomerName,
            CustomerPhone = request.CustomerPhone,
            DeliveryAddress = request.DeliveryAddress,
            Status = "pending",
            Total = total,
            CreatedAt = now,
            UpdatedAt = now
        };

        var orderItems = request.Items.Select(item => new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            Notes = item.Notes
        }).ToList();

        order.Items = orderItems;

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        // Reload with product data
        var createdOrder = await db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstAsync(o => o.Id == order.Id);

        var response = new Response(
            createdOrder.Id,
            createdOrder.Items.Select(i => new OrderItemDto(
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
            createdOrder.CustomerName,
            createdOrder.CustomerPhone,
            createdOrder.DeliveryAddress,
            createdOrder.Status,
            createdOrder.Total,
            createdOrder.CreatedAt,
            createdOrder.UpdatedAt);

        return Results.Created($"/api/orders/{order.Id}", response);
    }
}
