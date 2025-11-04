namespace Devlivery.WebApi.Features.Orders.Queries.GetAllOrders;

public static class GetAllOrdersEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("", async (GetAllOrdersHandler handler, CancellationToken ct) =>
        {
            var query = new GetAllOrdersQuery();

            var result = await handler.HandleAsync(query, ct);

            return result.IsFailed ? Results.Problem(result.Errors[0].Message) : Results.Ok(result.Value);
        });
    }
}