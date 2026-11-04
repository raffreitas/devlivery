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
        var totalsByStatus = await dbContext.Orders
            .AsNoTracking()
            .WhereDateInRange(o => o.CreatedAt, query.StartDate, query.EndDate)
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count(), Revenue = g.Sum(o => o.Total) })
            .ToListAsync(cancellationToken);

        var validTotals = totalsByStatus.Where(t => t.Status != OrderStatus.Canceled).ToList();

        var totalOrders = validTotals.Sum(t => t.Count);
        var totalRevenue = validTotals.Sum(t => t.Revenue);
        var pendingOrders = totalsByStatus
            .Where(t => t.Status is OrderStatus.Pending or OrderStatus.Preparing or OrderStatus.Ready)
            .Sum(t => t.Count);
        var deliveredOrders = totalsByStatus
            .Where(t => t.Status == OrderStatus.Delivered)
            .Sum(t => t.Count);
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