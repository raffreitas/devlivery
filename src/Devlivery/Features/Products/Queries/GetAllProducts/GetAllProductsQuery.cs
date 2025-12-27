using FluentResults;

using Mediator;

namespace Devlivery.Features.Products.Queries.GetAllProducts;

public sealed record GetAllProductsQuery : IQuery<Result<List<GetAllProductsResponse>>>;