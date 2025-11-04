using Devlivery.WebApi.Shared.Extensions;
using Devlivery.WebApi.Shared.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.WebApi.Features.Products.Queries.GetAllProducts;

public static class GetAllProductsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("", Handle)
            .Produces<ApiResponse<List<GetAllProductsResponse>>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Results<Ok<ApiResponse<List<GetAllProductsResponse>>>, BadRequest<ProblemDetails>>> Handle(
        GetAllProductsHandler handler, 
        CancellationToken ct)
    {
        var query = new GetAllProductsQuery();
        var result = await handler.HandleAsync(query, ct);

        return result.IsSuccess
            ? result.ToOk("Products retrieved successfully")
            : result.ToBadRequestProblem();
    }
}