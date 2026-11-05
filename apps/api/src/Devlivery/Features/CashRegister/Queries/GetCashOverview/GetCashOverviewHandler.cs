using Devlivery.Common.Handlers;
using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.Common.Enums;
using Devlivery.Infrastructure.Persistence.Context;

using FluentResults;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.CashRegister.Queries.GetCashOverview;

public sealed class GetCashOverviewHandler(ApplicationDbContext dbContext)
    : IQueryHandler<GetCashOverviewQuery, Result<GetCashOverviewResponse>>
{
    public async Task<Result<GetCashOverviewResponse>> HandleAsync(GetCashOverviewQuery query,
        CancellationToken cancellationToken)
    {
        var active = await dbContext.CashSessions.AsNoTracking().Include(s => s.Movements)
            .Where(s => s.Status == CashSessionStatus.Open)
            .OrderByDescending(s => s.StartAt).ThenByDescending(s => s.Id).FirstOrDefaultAsync(cancellationToken);
        var last = await dbContext.CashSessions.AsNoTracking().Include(s => s.Movements)
            .Where(s => s.Status == CashSessionStatus.Closed)
            .OrderByDescending(s => s.EndAt).ThenByDescending(s => s.Id).FirstOrDefaultAsync(cancellationToken);

        CashReceivable? receivable = null;
        if (active is not null)
        {
            // No creation-date filter: a receipt belongs to the session open at confirmation.
            var orders = await dbContext.Orders.AsNoTracking()
                .Where(o => o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Canceled)
                .Select(o => new
                {
                    o.Total,
                    PendingCash =
                        o.Payments.Where(p =>
                                p.PaymentMethod == PaymentMethod.Cash && p.PaymentStatus == PaymentStatus.Pending)
                            .Sum(p => (decimal?)p.Amount) ?? 0m,
                    Electronic =
                        o.Payments.Where(p =>
                                p.PaymentMethod != PaymentMethod.Cash && p.PaymentStatus != PaymentStatus.Cancelled)
                            .Sum(p => (decimal?)p.Amount) ?? 0m,
                    ConfirmedCash =
                        o.Payments.Where(p =>
                                p.PaymentMethod == PaymentMethod.Cash && p.PaymentStatus == PaymentStatus.Confirmed)
                            .Sum(p => (decimal?)p.Amount) ?? 0m
                }).ToArrayAsync(cancellationToken);

            var amounts = orders
                .Select(o => Math.Min(o.PendingCash, Math.Max(0m, o.Total - o.Electronic - o.ConfirmedCash)))
                .Where(amount => amount > 0m).ToArray();
            receivable = new CashReceivable(amounts.Sum(), amounts.Length);
        }

        return Result.Ok(new GetCashOverviewResponse(
            active is null ? null : CashSessionSummary.FromDomain(active),
            last is null ? null : CashSessionSummary.FromDomain(last), receivable));
    }
}