namespace Devlivery.Features.Dashboard.Queries.GetTopProducts;

public sealed record GetTopProductsQuery(DateOnly? StartDate, DateOnly? EndDate);