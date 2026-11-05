using Devlivery.Common.Pagination;

namespace Devlivery.Features.CashRegister.Queries.GetCashHistory;

public sealed record CashHistorySession(Guid Id, string Attendant, DateTime StartAt, DateTime? EndAt,
    string Status, int TotalOrders, decimal TotalRevenue, decimal? ClosingAmount, decimal? Difference);

public sealed record CashHistoryTotals(int ClosedCount, int OpenCount, decimal TotalRevenue,
    int TotalOrders, decimal TotalDifference, int SessionsWithDifference);

public sealed record GetCashHistoryResponse(CashHistorySession[] Sessions, CashHistoryTotals Summary);

public sealed record GetCashHistoryResult(PaginatedResult<CashHistorySession> Page, CashHistoryTotals Summary);
