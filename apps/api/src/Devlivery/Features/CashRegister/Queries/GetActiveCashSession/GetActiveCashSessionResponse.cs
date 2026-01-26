using Devlivery.Domain.Aggregates.CashRegister;
using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Domain.Common.Enums;

namespace Devlivery.Features.CashRegister.Queries.GetActiveCashSession;

public sealed record GetActiveCashSessionResponse(
    Guid Id,
    Guid AttendantId,
    string AttendantName,
    decimal OpeningAmount,
    decimal? ClosingAmount,
    decimal ExpectedCashAmount,
    decimal TotalRevenue,
    int TotalOrders,
    PaymentBreakdownDto[] PaymentBreakdown,
    DateTime StartAt,
    DateTime? EndAt,
    string Status,
    string? Notes)
{
    public static GetActiveCashSessionResponse FromDomain(CashSession cashSession)
    {
        var paymentBreakdown = cashSession.Movements
            .Where(m => (m.EntryType
                            is CashSessionEntryType.Payment
                            or CashSessionEntryType.Refund
                            or CashSessionEntryType.Change)
                        && m.PaymentMethod != null)
            .GroupBy(m => m.PaymentMethod)
            .Select(g =>
            {
                var payments = g.Where(m => m.EntryType == CashSessionEntryType.Payment).ToList();
                var refunds = g.Where(m => m.EntryType == CashSessionEntryType.Refund).ToList();
                var changes = g.Where(m => m.EntryType == CashSessionEntryType.Change).ToList();
                var breakdownAmount = payments.Sum(p => p.Amount)
                                      - refunds.Sum(r => r.Amount)
                                      - changes.Sum(c => c.Amount);

                return new PaymentBreakdownDto(
                    g.Key!.Value,
                    breakdownAmount,
                    payments.Count - refunds.Count);
            })
            .Where(pb => pb.Amount > 0)
            .ToArray();

        // Calcular total de pedidos líquidos (payments - refunds - changes)
        var totalPayments = cashSession.Movements.Count(m => m.EntryType == CashSessionEntryType.Payment);
        var totalRefunds = cashSession.Movements.Count(m => m.EntryType == CashSessionEntryType.Refund);
        var totalOrders = totalPayments - totalRefunds;

        return new GetActiveCashSessionResponse(
            cashSession.Id,
            cashSession.AttendantId,
            cashSession.AttendantName,
            cashSession.OpeningAmount,
            cashSession.ClosingAmount,
            cashSession.ExpectedCashAmount,
            cashSession.TotalRevenue,
            totalOrders,
            paymentBreakdown,
            cashSession.StartAt,
            cashSession.EndAt,
            cashSession.Status.ToString(),
            cashSession.Notes);
    }
}

public sealed record PaymentBreakdownDto(PaymentMethod Method, decimal Amount, int Count);