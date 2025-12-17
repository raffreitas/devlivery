using Devlivery.Features.CashRegister.Domain;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessions;

public sealed record GetCashSessionsQuery(DateTime? StartDate, DateTime? EndDate, CashSessionStatus? Status);