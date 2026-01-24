using FluentResults;

using Mediator;

namespace Devlivery.Features.Dashboard.Queries.GetDashboardStats;

public sealed record GetDashboardStatsQuery(DateTime? StartDate, DateTime? EndDate)
    : IQuery<Result<GetDashboardStatsResponse>>;