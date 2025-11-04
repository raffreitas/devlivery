using Devlivery.WebApi.Shared.Infrastructure.Database.Context;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.Orders.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusHandler(ApplicationDbContext dbContext)
{
    public async Task<Result<UpdateOrderStatusResponse>> HandleAsync(
        UpdateOrderStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken);

        if (order is null)
        {
            return Result.Fail("Pedido não encontrado");
        }

        order.Status = command.Status;
        order.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new UpdateOrderStatusResponse(
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

        return Result.Ok(response);
    }
}
