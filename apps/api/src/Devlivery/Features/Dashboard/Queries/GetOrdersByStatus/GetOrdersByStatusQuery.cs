namespace Devlivery.Features.Dashboard.Queries.GetOrdersByStatus;

public sealed record GetOrdersByStatusQuery(DateOnly? StartDate, DateOnly? EndDate);