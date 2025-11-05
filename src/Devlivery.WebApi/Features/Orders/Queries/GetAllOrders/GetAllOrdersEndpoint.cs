using Devlivery.WebApi.Shared.Extensions;
using Devlivery.WebApi.Shared.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.WebApi.Features.Orders.Queries.GetAllOrders;

public static class GetAllOrdersEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("", Handle)
            .Produces<ApiResponse<List<GetAllOrdersResponse>>>()
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Results<Ok<ApiResponse<List<GetAllOrdersResponse>>>, BadRequest<ProblemDetails>>> Handle(
        GetAllOrdersHandler handler,
        CancellationToken ct)
    {
        var query = new GetAllOrdersQuery();
        var result = await handler.HandleAsync(query, ct);

        return result.IsSuccess
            ? result.ToOk("Orders retrieved successfully")
            : result.ToBadRequestProblem();
    }
}