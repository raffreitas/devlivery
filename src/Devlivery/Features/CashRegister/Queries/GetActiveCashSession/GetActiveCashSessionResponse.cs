using Devlivery.Features.CashRegister.Domain;
using Devlivery.Features.CashRegister.Domain.Enums;
using Devlivery.Shared.Domain.Enums;

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
            .Where(m => (m.EntryType == CashSessionEntryType.Payment || m.EntryType == CashSessionEntryType.Refund) && m.PaymentMethod != null)
            .GroupBy(m => m.PaymentMethod)
            .Select(g => new PaymentBreakdownDto(
                g.Key!.Value,
                g.Sum(m => m.Amount),
                g.Count()))
            .Where(pb => pb.Amount > 0)
            .ToArray();

        return new GetActiveCashSessionResponse(
            cashSession.Id,
            cashSession.AttendantId,
            cashSession.AttendantName,
            cashSession.OpeningAmount,
            cashSession.ClosingAmount,
            cashSession.ExpectedCashAmount,
            cashSession.TotalRevenue,
            cashSession.TotalOrders,
            paymentBreakdown,
            cashSession.StartAt,
            cashSession.EndAt,
            cashSession.Status.ToString(),
            cashSession.Notes);
    }
}

public sealed record PaymentBreakdownDto(PaymentMethod Method, decimal Amount, int Count);