namespace Devlivery.Features.CashRegister.Queries.GetCashSessions;

public sealed record GetCashSessionsQuery(DateTime? StartDate, DateTime? EndDate, string? Status);
