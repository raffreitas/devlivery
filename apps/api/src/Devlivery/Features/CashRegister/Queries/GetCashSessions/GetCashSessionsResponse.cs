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
    DateTime StartAt,
    DateTime? EndAt,
    string Status,
    string? Notes)
{
    public static GetCashSessionsResponse FromDomain(CashSession cashSession)
    {
        return new GetCashSessionsResponse(
            cashSession.Id,
            cashSession.AttendantId,
            cashSession.AttendantName,
            cashSession.OpeningAmount,
            cashSession.ClosingAmount,
            cashSession.ExpectedCashAmount,
            cashSession.TotalRevenue,
            cashSession.TotalOrders,
            cashSession.StartAt,
            cashSession.EndAt,
            cashSession.Status.ToString(),
            cashSession.Notes);
    }
}