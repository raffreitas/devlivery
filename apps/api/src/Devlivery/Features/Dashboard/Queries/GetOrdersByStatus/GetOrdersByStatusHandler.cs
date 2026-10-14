using Devlivery.Common.Extensions;
using Devlivery.Common.Handlers;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Infrastructure.Persistence.Context;

using FluentResults;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.Dashboard.Queries.GetOrdersByStatus;

public sealed class GetOrdersByStatusHandler(ApplicationDbContext dbContext)
    : IQueryHandler<GetOrdersByStatusQuery, Result<GetOrdersByStatusResponse>>
{
    public async Task<Result<GetOrdersByStatusResponse>> HandleAsync(
        GetOrdersByStatusQuery query,
        CancellationToken cancellationToken)
    {
        var ordersQuery = dbContext.Orders
            .AsNoTracking()
            .AsQueryable();

        ordersQuery = ordersQuery.WhereDateInRange(o => o.CreatedAt, query.StartDate, query.EndDate);

        var orders = await ordersQuery.ToListAsync(cancellationToken);

        var pending = orders.Count(o => o.Status == OrderStatus.Pending);
        var preparing = orders.Count(o => o.Status == OrderStatus.Preparing);
        var ready = orders.Count(o => o.Status == OrderStatus.Ready);
        var delivered = orders.Count(o => o.Status == OrderStatus.Delivered);
        var canceled = orders.Count(o => o.Status == OrderStatus.Canceled);

        var response = new GetOrdersByStatusResponse(pending, preparing, ready, delivered, canceled);

        return Result.Ok(response);
    }
}