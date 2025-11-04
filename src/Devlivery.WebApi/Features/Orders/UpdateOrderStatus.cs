using Devlivery.WebApi.Shared.Infrastructure.Database.Context;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.Orders;

public static class UpdateOrderStatus
{
    public record Request(string Status);

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
            RuleFor(x => x.Status)
                .NotEmpty()
                .Must(status => new[] { "pending", "preparing", "ready", "delivered", "cancelled" }.Contains(status))
                .WithMessage("Status deve ser: pending, preparing, ready, delivered ou cancelled");
        }
    }

    public static async Task<IResult> Handle(
        Guid id,
        Request request,
        ApplicationDbContext db,
        IValidator<Request> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var order = await db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
        {
            return Results.NotFound();
        }

        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

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
