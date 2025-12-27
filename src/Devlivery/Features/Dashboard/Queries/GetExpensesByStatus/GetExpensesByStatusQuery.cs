using FluentResults;
using Mediator;

namespace Devlivery.Features.Dashboard.Queries.GetExpensesByStatus;

public sealed record GetExpensesByStatusQuery(DateOnly? StartDate, DateOnly? EndDate)
    : IQuery<Result<GetExpensesByStatusResponse>>;

