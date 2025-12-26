namespace Devlivery.Features.Dashboard.Queries.GetTopProducts;

public sealed record TopProductItem(string Name, int Quantity);

public sealed record GetTopProductsResponse(List<TopProductItem> Products);

