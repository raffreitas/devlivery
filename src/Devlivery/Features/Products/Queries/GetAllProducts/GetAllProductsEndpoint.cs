using Devlivery.Infrastructure.WebServer.Extensions;
using Devlivery.Infrastructure.WebServer.Models;

using Mediator;

namespace Devlivery.Features.Products.Queries.GetAllProducts;

public static class GetAllProductsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("", Handle)
            .Produces<ApiResponse<List<GetAllProductsResponse>>>()
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(ISender sender, CancellationToken ct)
    {
        var query = new GetAllProductsQuery();
        var result = await sender.Send(query, ct);

        return result.ToApiResult();
    }
}