using Devlivery.Features.CashRegister.Domain.Enums;

using Mediator;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessions;

public sealed record GetCashSessionsQuery(DateTime? StartDate, DateTime? EndDate, CashSessionStatus? Status)
    : IQuery<GetCashSessionsResponse[]>;