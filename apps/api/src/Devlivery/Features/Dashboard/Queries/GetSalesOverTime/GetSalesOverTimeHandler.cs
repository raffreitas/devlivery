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
        var timeZoneId = BrazilTime.TimeZone.Id;

        var salesByDate = await dbContext.Orders
            .AsNoTracking()
            .WhereDateInRange(o => o.CreatedAt, query.StartDate, query.EndDate)
            .Where(o => o.Status != OrderStatus.Canceled)
            .GroupBy(o => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(o.CreatedAt, timeZoneId).Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(o => o.Total) })
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        var data = salesByDate
            .Select(x => new SalesTimeSeriesItem(
                x.Date.ToString("yyyy-MM-dd"),
                x.Total))
            .ToList();

        var response = new GetSalesOverTimeResponse(data);

        return Result.Ok(response);
    }
}