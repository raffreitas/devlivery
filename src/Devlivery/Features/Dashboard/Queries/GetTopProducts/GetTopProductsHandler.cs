using Devlivery.Features.Orders.Domain.Enums;
using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.Persistence.Context;

using FluentResults;

using Mediator;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.Dashboard.Queries.GetTopProducts;

public sealed class GetTopProductsHandler(ApplicationDbContext dbContext)
    : IQueryHandler<GetTopProductsQuery, Result<GetTopProductsResponse>>
{
    public async ValueTask<Result<GetTopProductsResponse>> Handle(
        GetTopProductsQuery query,
        CancellationToken cancellationToken)
    {
        var ordersQuery = dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .AsQueryable();

        // Apply date filter
        ordersQuery = ordersQuery.WhereDateInRange(o => o.CreatedAt, query.StartDate, query.EndDate);

        // Filter out canceled orders
        ordersQuery = ordersQuery.Where(o => o.Status != OrderStatus.Canceled);

        var orders = await ordersQuery.ToListAsync(cancellationToken);

        // Get all product IDs from order items
        var productIds = orders
            .SelectMany(o => o.Items)
            .Select(i => i.ProductId)
            .Distinct()
            .ToHashSet();

        // Load products
        var products = await dbContext.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        var productsDictionary = products.ToDictionary(p => p.Id, p => p);

        // Aggregate quantities by product
        var productSales = orders
            .SelectMany(o => o.Items)
            .GroupBy(i => i.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Quantity = g.Sum(i => i.Quantity)
            })
            .OrderByDescending(x => x.Quantity)
            .Take(5)
            .ToList();

        // Map to response with product names
        var topProducts = productSales
            .Select(x => new TopProductItem(
                productsDictionary[x.ProductId].Name,
                x.Quantity))
            .ToList();

        var response = new GetTopProductsResponse(topProducts);

        return Result.Ok(response);
    }
}