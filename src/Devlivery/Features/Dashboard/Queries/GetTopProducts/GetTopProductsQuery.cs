using FluentResults;
using Mediator;

namespace Devlivery.Features.Dashboard.Queries.GetTopProducts;

public sealed record GetTopProductsQuery(DateTime? StartDate, DateTime? EndDate)
    : IQuery<Result<GetTopProductsResponse>>;

