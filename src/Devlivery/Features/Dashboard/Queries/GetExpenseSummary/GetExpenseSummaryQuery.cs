using FluentResults;
using Mediator;

namespace Devlivery.Features.Dashboard.Queries.GetExpenseSummary;

public sealed record GetExpenseSummaryQuery(DateOnly? StartDate, DateOnly? EndDate)
    : IQuery<Result<GetExpenseSummaryResponse>>;

