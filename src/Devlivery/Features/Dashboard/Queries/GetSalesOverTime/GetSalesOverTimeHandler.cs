using Devlivery.Features.Orders.Domain.Enums;
using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.Persistence.Context;

using FluentResults;

using Mediator;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.Dashboard.Queries.GetSalesOverTime;

public sealed class GetSalesOverTimeHandler(ApplicationDbContext dbContext)
    : IQueryHandler<GetSalesOverTimeQuery, Result<GetSalesOverTimeResponse>>
{
    private static readonly TimeZoneInfo BrazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public async ValueTask<Result<GetSalesOverTimeResponse>> Handle(
        GetSalesOverTimeQuery query,
        CancellationToken cancellationToken)
    {
        var ordersQuery = dbContext.Orders
            .AsNoTracking()
            .AsQueryable();

        // Apply date filter
        ordersQuery = ordersQuery.WhereDateInRange(o => o.CreatedAt, query.StartDate, query.EndDate);

        // Filter out canceled orders
        ordersQuery = ordersQuery.Where(o => o.Status != OrderStatus.Canceled);

        var orders = await ordersQuery.ToListAsync(cancellationToken);

        // Group by date (using local Brazil timezone)
        var salesByDate = orders
            .GroupBy(o =>
            {
                var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(o.CreatedAt, BrazilTimeZone);
                return localDateTime.Date;
            })
            .Select(g => new
            {
                Date = g.Key,
                Total = g.Sum(o => o.Total)
            })
            .OrderBy(x => x.Date)
            .ToList();

        // Format dates as "DD/MM" (pt-BR)
        var data = salesByDate
            .Select(x => new SalesTimeSeriesItem(
                x.Date.ToString("dd/MM"),
                x.Total))
            .ToList();

        var response = new GetSalesOverTimeResponse(data);

        return Result.Ok(response);
    }
}