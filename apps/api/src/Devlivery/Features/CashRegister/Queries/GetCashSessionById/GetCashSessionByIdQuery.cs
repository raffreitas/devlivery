using FluentResults;
using Mediator;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessionById;

public sealed record GetCashSessionByIdQuery(Guid Id) : IQuery<Result<GetCashSessionByIdResponse>>;