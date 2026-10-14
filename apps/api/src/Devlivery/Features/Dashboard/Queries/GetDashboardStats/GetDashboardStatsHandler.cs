using Devlivery.Common.Extensions;
using Devlivery.Common.Handlers;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Infrastructure.Persistence.Context;

using FluentResults;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.Dashboard.Queries.GetDashboardStats;

public sealed class GetDashboardStatsHandler(ApplicationDbContext dbContext)
    : IQueryHandler<GetDashboardStatsQuery, Result<GetDashboardStatsResponse>>
{
    public async Task<Result<GetDashboardStatsResponse>> HandleAsync(
        GetDashboardStatsQuery query,
        CancellationToken cancellationToken)
    {
        var ordersQuery = dbContext.Orders
            .AsNoTracking()
            .AsQueryable();

        ordersQuery = ordersQuery.WhereDateInRange(o => o.CreatedAt, query.StartDate, query.EndDate);

        var orders = await ordersQuery.ToListAsync(cancellationToken);

        var validOrders = orders.Where(o => o.Status != OrderStatus.Canceled).ToList();

        var totalOrders = validOrders.Count;
        var totalRevenue = validOrders.Sum(o => o.Total);
        var pendingOrders = orders.Count(o =>
            o.Status == OrderStatus.Pending ||
            o.Status == OrderStatus.Preparing ||
            o.Status == OrderStatus.Ready);
        var deliveredOrders = orders.Count(o => o.Status == OrderStatus.Delivered);
        var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

        var response = new GetDashboardStatsResponse(
            totalOrders,
            totalRevenue,
            pendingOrders,
            deliveredOrders,
            averageOrderValue);

        return Result.Ok(response);
    }
}