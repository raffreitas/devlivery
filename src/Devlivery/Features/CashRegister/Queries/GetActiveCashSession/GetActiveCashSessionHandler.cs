using Devlivery.Features.CashRegister.Domain;
using Devlivery.Features.Orders.Domain;
using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Infrastructure.Persistence.Context;

using FluentResults;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.CashRegister.Queries.GetActiveCashSession;

public sealed class GetActiveCashSessionHandler(ApplicationDbContext dbContext)
{
    public async Task<Result<GetActiveCashSessionResponse>> HandleAsync(
        GetActiveCashSessionQuery query,
        CancellationToken cancellationToken = default)
    {
        var cashSession = await dbContext.CashSessions
            .AsNoTracking()
            .Where(cs => cs.Status == CashSessionStatus.Open)
            .OrderByDescending(cs => cs.StartAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (cashSession is null)
        {
            return Result.Fail<GetActiveCashSessionResponse>(new NotFoundError("Caixa não encontrado."));
        }

        // Calculate sales within session period
        var sessionOrders = await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= cashSession.StartAt &&
                        o.Status != OrderStatus.Canceled)
            .ToListAsync(cancellationToken);

        // Calculate totals
        var totalRevenue = sessionOrders.Sum(o => o.Total);
        var totalOrders = sessionOrders.Count;

        // Calculate payment breakdown
        var paymentBreakdown = sessionOrders
            .GroupBy(o => o.PaymentMethod)
            .Select(g => new PaymentBreakdownItem(
                g.Key.ToString(),
                g.Sum(o => o.Total),
                g.Count()))
            .ToList();

        // Calculate expected cash (opening + cash only)
        var cashSales = sessionOrders
            .Where(o => o.PaymentMethod == PaymentMethod.Cash)
            .Sum(o => o.Total);

        // Get deposits made during the session
        var totalDeposits = await dbContext.CashDeposits
            .Where(cd => cd.CashSessionId == cashSession.Id)
            .SumAsync(cd => cd.Amount, cancellationToken);

        var expectedCashAmount = cashSession.OpeningAmount + totalDeposits + cashSales;

        return Result.Ok(GetActiveCashSessionResponse.FromDomain(cashSession, expectedCashAmount));
    }
}