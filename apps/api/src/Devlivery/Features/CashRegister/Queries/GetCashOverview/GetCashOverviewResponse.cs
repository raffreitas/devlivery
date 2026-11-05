namespace Devlivery.Features.CashRegister.Queries.GetCashOverview;

public sealed record GetCashOverviewResponse(
    CashSessionSummary? ActiveSession,
    CashSessionSummary? LastClosedSession,
    CashReceivable? ReceivableCash);

public sealed record CashReceivable(decimal Amount, int OrderCount);