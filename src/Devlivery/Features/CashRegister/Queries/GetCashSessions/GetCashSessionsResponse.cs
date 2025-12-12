using System.Collections.ObjectModel;
using Devlivery.Features.CashRegister.Domain;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessions;

public sealed record GetCashSessionsResponse(
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
    public static GetCashSessionsResponse FromDomain(
        CashSession cashSession,
        decimal? expectedCashAmount = null)
    {
        var payments = cashSession.PaymentBreakdown
            .Select(pb => new PaymentBreakdownDto(pb.Method, pb.Amount, pb.Count))
            .ToList();

        // Calculate expected cash from payment breakdown if not provided
        var calculatedExpectedCash = expectedCashAmount ??
                                     cashSession.OpeningAmount + cashSession.PaymentBreakdown
                                         .Where(pb => pb.Method.Equals("cash", StringComparison.OrdinalIgnoreCase))
                                         .Sum(pb => pb.Amount);

        return new GetCashSessionsResponse(
            cashSession.Id,
            cashSession.AttendantId,
            cashSession.AttendantName,
            cashSession.OpeningAmount,
            cashSession.ClosingAmount,
            calculatedExpectedCash,
            cashSession.TotalRevenue,
            cashSession.TotalOrders,
            new ReadOnlyCollection<PaymentBreakdownDto>(payments),
            cashSession.StartAt,
            cashSession.EndAt,
            cashSession.Status.ToString().ToLowerInvariant(),
            cashSession.Notes);
    }
}

