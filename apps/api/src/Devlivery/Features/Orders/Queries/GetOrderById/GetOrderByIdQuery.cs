using FluentResults;

using Mediator;

namespace Devlivery.Features.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid Id) : IQuery<Result<GetOrderByIdResponse>>;