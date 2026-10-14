namespace Devlivery.Features.Dashboard.Queries.GetExpensesByCategory;

public sealed record GetExpensesByCategoryQuery(DateOnly? StartDate, DateOnly? EndDate);