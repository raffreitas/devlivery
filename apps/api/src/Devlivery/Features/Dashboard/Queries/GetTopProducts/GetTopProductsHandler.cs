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
        var productSales = await dbContext.Orders
            .AsNoTracking()
            .WhereDateInRange(o => o.CreatedAt, query.StartDate, query.EndDate)
            .Where(o => o.Status != OrderStatus.Canceled)
            .SelectMany(o => o.Items)
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(i => i.Quantity) })
            .ToListAsync(cancellationToken);

        var productIds = productSales.Select(s => s.ProductId).ToList();

        var productNames = await dbContext.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

        var topProducts = productSales
            .Select(s => new TopProductItem(productNames[s.ProductId], s.Quantity))
            .OrderByDescending(p => p.Quantity)
            .ThenBy(p => p.Name)
            .ToList();

        var response = new GetTopProductsResponse(topProducts);

        return Result.Ok(response);
    }
}