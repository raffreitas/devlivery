using Devlivery.Common.Extensions;
using Devlivery.Common.Handlers;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.Common.Enums;
using Devlivery.Infrastructure.Persistence.Context;

using FluentResults;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.Dashboard.Queries.GetPaymentBreakdown;

public sealed class GetPaymentBreakdownHandler(ApplicationDbContext dbContext)
    : IQueryHandler<GetPaymentBreakdownQuery, Result<GetPaymentBreakdownResponse>>
{
    public async Task<Result<GetPaymentBreakdownResponse>> HandleAsync(
        GetPaymentBreakdownQuery query,
        CancellationToken cancellationToken)
    {
        var ordersQuery = dbContext.Orders
            .AsNoTracking()
            .WhereDateInRange(o => o.CreatedAt, query.StartDate, query.EndDate)
            .Where(o => o.Status != OrderStatus.Canceled);

        // Consider only confirmed payments in the breakdown; unconfirmed payments are not yet received.
        var breakdown = await ordersQuery
            .SelectMany(o => o.Payments)
            .Where(p => p.PaymentStatus == PaymentStatus.Confirmed)
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new { Method = g.Key, Total = g.Sum(p => p.Amount) })
            .ToDictionaryAsync(g => g.Method, g => g.Total, cancellationToken);

        var allMethods = Enum.GetValues<PaymentMethod>();
        var completeBreakdown = allMethods.ToDictionary(
            method => method,
            method => breakdown.GetValueOrDefault(method, 0m));

        // Total change given back to customers should be subtracted from cash payments
        var totalChange = await ordersQuery.SumAsync(o => o.Change, cancellationToken);
        completeBreakdown[PaymentMethod.Cash] = Math.Max(0m, completeBreakdown[PaymentMethod.Cash] - totalChange);

        var total = completeBreakdown.Values.Sum();

        var response = new GetPaymentBreakdownResponse(completeBreakdown, total);

        return Result.Ok(response);
    }
}