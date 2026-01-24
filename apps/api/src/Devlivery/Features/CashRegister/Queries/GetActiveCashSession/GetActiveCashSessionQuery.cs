using FluentResults;
using Mediator;

namespace Devlivery.Features.CashRegister.Queries.GetActiveCashSession;

public sealed record GetActiveCashSessionQuery : IQuery<Result<GetActiveCashSessionResponse>>;