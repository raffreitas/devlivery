using FluentResults;

using Mediator;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessionDeposits;

public sealed record GetCashSessionDepositsQuery(Guid CashSessionId) : IQuery<Result<GetCashSessionDepositsResponse[]>>;