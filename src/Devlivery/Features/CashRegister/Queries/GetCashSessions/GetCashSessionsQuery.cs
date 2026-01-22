using Devlivery.Domain.Aggregates.CashRegister.Enums;

using FluentResults;

using Mediator;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessions;

public sealed record GetCashSessionsQuery(DateTime? StartDate, DateTime? EndDate, CashSessionStatus? Status)
    : IQuery<Result<GetCashSessionsResponse[]>>;