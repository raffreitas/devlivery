using Devlivery.Features.CashRegister.Domain;

namespace Devlivery.Features.CashRegister.Commands.CloseCashSession;

public sealed record PaymentBreakdownItem(string Method, decimal Amount, int Count);

public sealed record CloseCashSessionResponse(
    Guid Id,
    string AttendantName,
    DateTime StartAt,
    DateTime? EndAt,
    decimal OpeningAmount,
    decimal ClosingAmount,
    decimal TotalRevenue,
    int TotalOrders,
    string Status,
    List<PaymentBreakdownItem> PaymentBreakdown,
    decimal ExpectedCashAmount,
    string? Notes)
{
    public static CloseCashSessionResponse FromDomain(CashSession cashSession, decimal expectedCashAmount)
    {
        var paymentBreakdownItems = cashSession.PaymentBreakdown?
            .Select(pb => new PaymentBreakdownItem(pb.Method, pb.Amount, pb.Count))
            .ToList() ?? new List<PaymentBreakdownItem>();

        return new CloseCashSessionResponse(
            cashSession.Id,
            cashSession.AttendantName,
            cashSession.StartAt,
            cashSession.EndAt,
            cashSession.OpeningAmount,
            cashSession.ClosingAmount ?? 0,
            cashSession.TotalRevenue,
            cashSession.TotalOrders,
            cashSession.Status.ToString().ToLowerInvariant(),
            paymentBreakdownItems,
            expectedCashAmount,
            cashSession.Notes);
    }
}
