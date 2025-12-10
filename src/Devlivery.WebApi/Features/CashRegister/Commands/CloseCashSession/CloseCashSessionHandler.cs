using Devlivery.WebApi.Features.CashRegister.Domain;
using Devlivery.WebApi.Features.CashRegister.DTOs;
using Devlivery.WebApi.Features.CashRegister.Errors;
using Devlivery.WebApi.Features.Orders.Domain;
using Devlivery.WebApi.Shared.Database.Context;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.CashRegister.Commands.CloseCashSession;

public sealed class CloseCashSessionHandler(ApplicationDbContext dbContext)
{
    public async Task<Result<CashSessionResponse>> HandleAsync(
        CloseCashSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var cashSession = await dbContext.CashSessions
            .FirstOrDefaultAsync(cs => cs.Id == command.Id, cancellationToken);

        if (cashSession is null)
        {
            return Result.Fail<CashSessionResponse>(CashRegisterErrors.CashSessionNotFound);
        }

        if (cashSession.Status == CashSessionStatus.Closed)
        {
            return Result.Fail<CashSessionResponse>(CashRegisterErrors.CashSessionAlreadyClosed);
        }

        // Get all orders within the cash session period (exclude canceled)
        var sessionStart = cashSession.StartAt;
        var sessionEnd = DateTime.UtcNow;

        var sessionOrders = await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= sessionStart && o.CreatedAt <= sessionEnd && o.Status != OrderStatus.Canceled)
            .ToListAsync(cancellationToken);

        // Calculate totals
        var totalRevenue = sessionOrders.Sum(o => o.Total);
        var totalOrders = sessionOrders.Count;

        // Calculate payment breakdown
        var paymentBreakdown = sessionOrders
            .GroupBy(o => o.PaymentMethod.ToString())
            .Select(g => new PaymentBreakdownItem(
                g.Key,
                g.Sum(o => o.Total),
                g.Count()))
            .ToList();

        // ✅ IMPORTANTE: Recalcular ExpectedCashAmount ANTES de fechar
        // Formula: Opening + Deposits + CashSales
        // Motivo: PaymentBreakdown só é disponível aqui no fechamento
        var totalDeposits = await dbContext.CashDeposits
            .Where(cd => cd.CashSessionId == cashSession.Id)
            .SumAsync(cd => cd.Amount, cancellationToken);

        var cashSales = paymentBreakdown
            .Where(pb => pb.Method.Equals(nameof(PaymentMethod.Cash), StringComparison.OrdinalIgnoreCase))
            .Sum(pb => pb.Amount);

        var expectedCashAmount = cashSession.OpeningAmount + totalDeposits + cashSales;
        cashSession.UpdateExpectedCashAmount(expectedCashAmount);

        // Update session with calculated totals
        cashSession.UpdateTotals(totalRevenue, totalOrders, paymentBreakdown);
        cashSession.Close(command.ClosingAmount, command.Notes);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok(CashSessionResponse.FromDomain(cashSession));
    }
}