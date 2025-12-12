using System.Collections.ObjectModel;
using Devlivery.Features.CashRegister.Domain;

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
    IReadOnlyCollection<PaymentBreakdownDto> PaymentBreakdown,
    DateTime StartAt,
    DateTime? EndAt,
    string Status,
    string? Notes)
{
    public static GetActiveCashSessionResponse FromDomain(
        CashSession cashSession,
        decimal expectedCashAmount)
    {
        var payments = cashSession.PaymentBreakdown
            .Select(pb => new PaymentBreakdownDto(pb.Method, pb.Amount, pb.Count))
            .ToList();

        return new GetActiveCashSessionResponse(
            cashSession.Id,
            cashSession.AttendantId,
            cashSession.AttendantName,
            cashSession.OpeningAmount,
            cashSession.ClosingAmount,
            expectedCashAmount,
            cashSession.TotalRevenue,
            cashSession.TotalOrders,
            new ReadOnlyCollection<PaymentBreakdownDto>(payments),
            cashSession.StartAt,
            cashSession.EndAt,
            cashSession.Status.ToString().ToLowerInvariant(),
            cashSession.Notes);
    }
}

