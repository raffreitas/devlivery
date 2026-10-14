using Devlivery.Common.Extensions;
using Devlivery.Common.Handlers;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Infrastructure.Persistence.Context;

using FluentResults;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.Dashboard.Queries.GetTopProducts;

public sealed class GetTopProductsHandler(ApplicationDbContext dbContext)
    : IQueryHandler<GetTopProductsQuery, Result<GetTopProductsResponse>>
{
    public async Task<Result<GetTopProductsResponse>> HandleAsync(
        GetTopProductsQuery query,
        CancellationToken cancellationToken)
    {
        var ordersQuery = dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .AsQueryable();

        ordersQuery = ordersQuery.WhereDateInRange(o => o.CreatedAt, query.StartDate, query.EndDate);

        ordersQuery = ordersQuery.Where(o => o.Status != OrderStatus.Canceled);

        var orders = await ordersQuery.ToListAsync(cancellationToken);

        var productIds = orders
            .SelectMany(o => o.Items)
            .Select(i => i.ProductId)
            .Distinct()
            .ToHashSet();

        var products = await dbContext.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        var productsDictionary = products.ToDictionary(p => p.Id, p => p);

        var productSales = orders
            .SelectMany(o => o.Items)
            .GroupBy(i => i.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Quantity = g.Sum(i => i.Quantity)
            })
            .OrderByDescending(x => x.Quantity)
            .ToList();

        var topProducts = productSales
            .Select(x => new TopProductItem(
                productsDictionary[x.ProductId].Name,
                x.Quantity))
            .ToList();

        var response = new GetTopProductsResponse(topProducts);

        return Result.Ok(response);
    }
}