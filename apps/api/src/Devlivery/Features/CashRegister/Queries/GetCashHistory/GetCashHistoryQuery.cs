namespace Devlivery.Features.CashRegister.Queries.GetCashHistory;

public sealed record GetCashHistoryQuery(DateOnly Start, DateOnly End, int Page = 1, int PageSize = 50);
