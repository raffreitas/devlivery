using Devlivery.Domain.Aggregates.CashRegister.Enums;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessions;

public sealed record GetCashSessionsQuery(DateOnly? StartDate, DateOnly? EndDate, CashSessionStatus? Status);