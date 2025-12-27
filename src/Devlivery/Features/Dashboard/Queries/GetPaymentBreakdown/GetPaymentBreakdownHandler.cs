using Devlivery.Features.Orders.Domain.Enums;
using Devlivery.Shared.Extensions;
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
            .AsNoTracking()
            .AsQueryable();

        // Apply date filter
        ordersQuery = ordersQuery.WhereDateInRange(o => o.CreatedAt, query.StartDate, query.EndDate);

        // Filter out canceled orders
        ordersQuery = ordersQuery.Where(o => o.Status != OrderStatus.Canceled);

        var orders = await ordersQuery.ToListAsync(cancellationToken);

        var breakdown = orders
            .GroupBy(o => o.PaymentMethod)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(o => o.Total));

        // Ensure all payment methods are present with 0 value
        var allMethods = Enum.GetValues<PaymentMethod>();
        var completeBreakdown = allMethods.ToDictionary(
            method => method,
            method => breakdown.GetValueOrDefault(method, 0));

        var total = completeBreakdown.Values.Sum();

        var response = new GetPaymentBreakdownResponse(completeBreakdown, total);

        return Result.Ok(response);
    }
}

