using Devlivery.Domain.Aggregates.CashRegister.Enums;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessions;

public sealed record GetCashSessionsQuery(DateTime? StartDate, DateTime? EndDate, CashSessionStatus? Status);