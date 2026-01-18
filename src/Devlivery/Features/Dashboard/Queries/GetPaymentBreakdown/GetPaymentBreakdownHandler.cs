using Devlivery.Features.Orders.Domain.Enums;
using Devlivery.Shared.CrossCutting.Extensions;
using Devlivery.Shared.Domain.Enums;
using Devlivery.Shared.Infrastructure.Persistence.Context;

using FluentResults;

using Mediator;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.Dashboard.Queries.GetPaymentBreakdown;

public sealed class GetPaymentBreakdownHandler(ApplicationDbContext dbContext)
    : IQueryHandler<GetPaymentBreakdownQuery, Result<GetPaymentBreakdownResponse>>
{
    public async ValueTask<Result<GetPaymentBreakdownResponse>> Handle(
        GetPaymentBreakdownQuery query,
        CancellationToken cancellationToken)
    {
        var ordersQuery = dbContext.Orders
            .Include(o => o.Payments)
            .AsNoTracking()
            .AsQueryable();

        // Apply date filter
        ordersQuery = ordersQuery.WhereDateInRange(o => o.CreatedAt, query.StartDate, query.EndDate);

        // Filter out canceled orders
        ordersQuery = ordersQuery.Where(o => o.Status != OrderStatus.Canceled);

        var orders = await ordersQuery.ToListAsync(cancellationToken);

        // Subtract total change (troco) from payment totals. The change is stored on orders and
        // should not be counted as part of sales (it is money returned to customer).
        var breakdown = orders
            .SelectMany(o => o.Payments.Where(p => p.PaymentStatus != PaymentStatus.Cancelled))
            .GroupBy(o => o.PaymentMethod)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(o => o.Amount));

        // Ensure all payment methods are present with 0 value
        var allMethods = Enum.GetValues<PaymentMethod>();
        var completeBreakdown = allMethods.ToDictionary(
            method => method,
            method => breakdown.GetValueOrDefault(method, 0m));

        // Total change given back to customers should be subtracted from cash payments
        var totalChange = orders.Sum(o => o.Change);
        completeBreakdown[PaymentMethod.Cash] = Math.Max(0m, completeBreakdown[PaymentMethod.Cash] - totalChange);

        var total = completeBreakdown.Values.Sum();

        var response = new GetPaymentBreakdownResponse(completeBreakdown, total);

        return Result.Ok(response);
    }
}