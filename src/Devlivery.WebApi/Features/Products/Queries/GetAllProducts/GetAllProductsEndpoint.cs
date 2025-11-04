namespace Devlivery.WebApi.Features.Products.Queries.GetAllProducts;

public static class GetAllProductsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("", async (GetAllProductsHandler handler, CancellationToken ct) =>
        {
            var query = new GetAllProductsQuery();

            var result = await handler.HandleAsync(query, ct);

            return result.IsFailed ? Results.Problem(result.Errors[0].Message) : Results.Ok(result.Value);
        });
    }
}