namespace Devlivery.Features.CashRegister.Queries.GetCashHistory;

internal sealed record CashHistorySummary(int TotalCount, int ClosedCount, int OpenCount, decimal TotalRevenue,
    int TotalOrders, decimal TotalDifference, int SessionsWithDifference)
{
    public CashHistoryTotals ToTotals() => new(ClosedCount, OpenCount, TotalRevenue, TotalOrders,
        TotalDifference, SessionsWithDifference);
}
