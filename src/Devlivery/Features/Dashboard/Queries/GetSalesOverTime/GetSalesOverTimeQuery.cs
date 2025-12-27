using FluentResults;
using Mediator;

namespace Devlivery.Features.Dashboard.Queries.GetSalesOverTime;

public sealed record GetSalesOverTimeQuery(DateTime? StartDate, DateTime? EndDate)
    : IQuery<Result<GetSalesOverTimeResponse>>;

