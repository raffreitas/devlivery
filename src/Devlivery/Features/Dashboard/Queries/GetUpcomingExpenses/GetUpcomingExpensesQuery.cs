using FluentResults;

using Mediator;

namespace Devlivery.Features.Dashboard.Queries.GetUpcomingExpenses;

public sealed record GetUpcomingExpensesQuery(int Days = 7)
    : IQuery<Result<GetUpcomingExpensesResponse>>;