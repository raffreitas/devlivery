using System.Collections.ObjectModel;

using Devlivery.Features.CashRegister.Domain;
using Devlivery.Features.Orders.Domain.Enums;

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
    public static GetActiveCashSessionResponse FromDomain(CashSession cashSession)
    {
        var payments = cashSession.PaymentBreakdown
            .Select(pb => new PaymentBreakdownDto(pb.Method, pb.Amount, pb.Count))
            .ToList();

        var cashSales = payments
            .Where(o => o.Method == nameof(PaymentMethod.Cash))
            .Sum(o => o.Amount);

        var totalDeposits = cashSession.Deposits.Sum(o => o.Amount);

        var expectedCashAmount = cashSession.OpeningAmount + totalDeposits + cashSales;

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
            cashSession.Status.ToString(),
            cashSession.Notes);
    }
}