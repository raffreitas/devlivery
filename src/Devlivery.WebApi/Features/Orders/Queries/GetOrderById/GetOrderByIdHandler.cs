using Devlivery.WebApi.Shared.Database.Context;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdHandler(ApplicationDbContext dbContext)
{
    public async Task<Result<GetOrderByIdResponse>> HandleAsync(
        GetOrderByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == query.Id, cancellationToken);

        if (order is null)
        {
            return Result.Fail("Pedido não encontrado");
        }

        var response = new GetOrderByIdResponse(
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
            order.DeliveryFee,
            order.PaymentMethod.ToString(),
            order.CreatedAt,
            order.UpdatedAt);

        return Result.Ok(response);
    }
}