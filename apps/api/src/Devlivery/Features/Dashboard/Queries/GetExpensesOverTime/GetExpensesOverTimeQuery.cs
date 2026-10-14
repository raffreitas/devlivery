namespace Devlivery.Features.Dashboard.Queries.GetExpensesOverTime;

public sealed record GetExpensesOverTimeQuery(DateOnly? StartDate, DateOnly? EndDate);