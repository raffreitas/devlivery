using Devlivery.Shared.CrossCutting.Extensions;
using Devlivery.Shared.Infrastructure.Persistence.Context;

using FluentResults;

using Mediator;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.Orders.Queries.GetAllOrders;

public sealed class GetAllOrdersHandler(ApplicationDbContext dbContext)
    : IQueryHandler<GetAllOrdersQuery, Result<List<GetAllOrdersResponse>>>
{
    public async ValueTask<Result<List<GetAllOrdersResponse>>> Handle(
        GetAllOrdersQuery query,
        CancellationToken cancellationToken)
    {
        var ordersQuery = dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .AsQueryable();

        // Date filtering: Convert local Brazil time (BRT/BRST) to UTC before querying
        // Database stores all dates in UTC, but filters are expected in local time (America/Sao_Paulo)
        ordersQuery = ordersQuery.WhereDateInRange(o => o.CreatedAt, query.StartDate, query.EndDate);

        if (query.PaymentMethod is not null)
        {
            ordersQuery = ordersQuery.Where(o => o.Payments.Any(p => p.PaymentMethod == query.PaymentMethod));
        }

        var orders = await ordersQuery
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        var productIds = orders
            .SelectMany(o => o.Items)
            .Select(i => i.ProductId)
            .ToHashSet();
        var products = await dbContext.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        var productsDictionary = products.ToDictionary(p => p.Id, p => p);

        var response = orders.Select(o => new GetAllOrdersResponse(
            o.Id,
            o.Items.Select(i => new OrderItemDto(
                new ProductDto(
                    productsDictionary[i.ProductId].Id,
                    productsDictionary[i.ProductId].Name,
                    productsDictionary[i.ProductId].Description,
                    productsDictionary[i.ProductId].Price,
                    productsDictionary[i.ProductId].Category,
                    productsDictionary[i.ProductId].Available,
                    productsDictionary[i.ProductId].CreatedAt,
                    productsDictionary[i.ProductId].UpdatedAt),
                i.Quantity,
                i.Notes)).OrderByDescending(x => x.Quantity).ToArray(),
            o.Customer.Name,
            o.Customer.Phone?.Number,
            o.DeliveryAddress.FullAddress,
            o.Notes,
            o.Status.ToString(),
            o.Total,
            o.DeliveryFee,
            o.Payments.Select(p => new OrderPaymentDto(p.Id, p.Amount, p.PaymentMethod.ToString())).ToArray(),
            o.CreatedAt,
            o.UpdatedAt)).ToList();

        return Result.Ok(response);
    }
}