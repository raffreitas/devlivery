using Devlivery.Common.Extensions;
using Devlivery.Common.Handlers;
using Devlivery.Common.Time;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Infrastructure.Persistence.Context;

using FluentResults;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.Dashboard.Queries.GetSalesOverTime;

public sealed class GetSalesOverTimeHandler(ApplicationDbContext dbContext)
    : IQueryHandler<GetSalesOverTimeQuery, Result<GetSalesOverTimeResponse>>
{
    public async Task<Result<GetSalesOverTimeResponse>> HandleAsync(
        GetSalesOverTimeQuery query,
        CancellationToken cancellationToken)
    {
        var ordersQuery = dbContext.Orders
            .AsNoTracking()
            .AsQueryable();

        ordersQuery = ordersQuery.WhereDateInRange(o => o.CreatedAt, query.StartDate, query.EndDate);

        ordersQuery = ordersQuery.Where(o => o.Status != OrderStatus.Canceled);

        var orders = await ordersQuery.ToListAsync(cancellationToken);

        var salesByDate = orders
            .GroupBy(o => o.CreatedAt.ToBrazilTime().Date)
            .Select(g => new
            {
                Date = g.Key,
                Total = g.Sum(o => o.Total)
            })
            .OrderBy(x => x.Date)
            .ToList();

        var data = salesByDate
            .Select(x => new SalesTimeSeriesItem(
                x.Date.ToString("yyyy-MM-dd"),
                x.Total))
            .ToList();

        var response = new GetSalesOverTimeResponse(data);

        return Result.Ok(response);
    }
}