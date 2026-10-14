namespace Devlivery.Features.Dashboard.Queries.GetDashboardStats;

public sealed record GetDashboardStatsQuery(DateOnly? StartDate, DateOnly? EndDate);