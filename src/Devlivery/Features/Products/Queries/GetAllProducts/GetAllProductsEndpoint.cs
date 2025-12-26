using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.Products.Queries.GetAllProducts;

public static class GetAllProductsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("", Handle)
            .Produces<ApiResponse<List<GetAllProductsResponse>>>()
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Ok<ApiResponse<List<GetAllProductsResponse>>>> Handle(
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetAllProductsQuery();
        var result = await sender.Send(query, ct);

        return result.ToOk();
    }
}