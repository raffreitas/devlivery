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
        var countsByStatus = await dbContext.Orders
            .AsNoTracking()
            .WhereDateInRange(o => o.CreatedAt, query.StartDate, query.EndDate)
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Status, g => g.Count, cancellationToken);

        var pending = countsByStatus.GetValueOrDefault(OrderStatus.Pending);
        var preparing = countsByStatus.GetValueOrDefault(OrderStatus.Preparing);
        var ready = countsByStatus.GetValueOrDefault(OrderStatus.Ready);
        var delivered = countsByStatus.GetValueOrDefault(OrderStatus.Delivered);
        var canceled = countsByStatus.GetValueOrDefault(OrderStatus.Canceled);

        var response = new GetOrdersByStatusResponse(pending, preparing, ready, delivered, canceled);

        return Result.Ok(response);
    }
}