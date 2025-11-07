using Devlivery.WebApi.Shared.Database.Context;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.Orders.Queries.GetAllOrders;

public sealed class GetAllOrdersHandler(ApplicationDbContext dbContext)
{
    public async Task<Result<List<GetAllOrdersResponse>>> HandleAsync(
        GetAllOrdersQuery query,
        CancellationToken cancellationToken = default)
    {
        var orders = await dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        var response = orders.Select(o => new GetAllOrdersResponse(
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
            o.DeliveryFee,
            o.PaymentMethod.ToString(),
            o.CreatedAt,
            o.UpdatedAt)).ToList();

        return Result.Ok(response);
    }
}