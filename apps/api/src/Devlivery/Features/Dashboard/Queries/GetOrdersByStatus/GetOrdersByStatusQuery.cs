namespace Devlivery.Features.Dashboard.Queries.GetOrdersByStatus;

public sealed record GetOrdersByStatusQuery(DateTime? StartDate, DateTime? EndDate);