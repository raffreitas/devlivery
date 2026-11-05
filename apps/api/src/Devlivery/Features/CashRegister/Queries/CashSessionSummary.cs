using Devlivery.Domain.Aggregates.CashRegister;
using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Domain.Common.Enums;
using Devlivery.Features.CashRegister.Queries.GetActiveCashSession;

namespace Devlivery.Features.CashRegister.Queries;

public sealed record CashSessionSummary(
    Guid Id,
    string Status,
    string Attendant,
    decimal OpeningAmount,
    decimal ExpectedCashAmount,
    decimal TotalRevenue,
    int TotalOrders,
    decimal DepositsTotal,
    decimal CashPayments,
    decimal CashRefunds,
    decimal ChangeAmount,
    decimal NetCashReceived,
    decimal? ClosingAmount,
    decimal? Difference,
    DateTime StartAt,
    DateTime? EndAt,
    string? Notes,
    PaymentBreakdownDto[] PaymentBreakdown)
{
    public static CashSessionSummary FromDomain(CashSession session) => new(
        session.Id,
        session.Status.ToString(),
        session.AttendantName,
        session.OpeningAmount,
        session.ExpectedCashAmount,
        session.TotalRevenue,
        session.TotalOrders,
        session.TotalDeposits(),
        session.Movements
            .Where(m => m.EntryType == CashSessionEntryType.Payment && m.PaymentMethod == PaymentMethod.Cash)
            .Sum(m => m.Amount),
        session.Movements
            .Where(m => m.EntryType == CashSessionEntryType.Refund && m.PaymentMethod == PaymentMethod.Cash)
            .Sum(m => m.Amount),
        session.Movements.Where(m => m.EntryType == CashSessionEntryType.Change).Sum(m => m.Amount),
        session.TotalCashPayments(),
        session.ClosingAmount,
        session.ClosingAmount.HasValue ? session.ClosingAmount.Value - session.ExpectedCashAmount : null,
        session.StartAt,
        session.EndAt,
        session.Notes,
        GetActiveCashSessionResponse.FromDomain(session).PaymentBreakdown);
}
