namespace Devlivery.Features.Dashboard.Queries.GetSalesOverTime;

public sealed record GetSalesOverTimeQuery(DateOnly? StartDate, DateOnly? EndDate);