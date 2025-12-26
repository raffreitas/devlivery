using FluentResults;
using Mediator;

namespace Devlivery.Features.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : IQuery<Result<GetProductByIdResponse>>;
