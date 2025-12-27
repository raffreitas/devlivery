using FluentResults;
using Mediator;

namespace Devlivery.Features.Dashboard.Queries.GetExpensesByCategory;

public sealed record GetExpensesByCategoryQuery(DateOnly? StartDate, DateOnly? EndDate)
    : IQuery<Result<GetExpensesByCategoryResponse>>;

