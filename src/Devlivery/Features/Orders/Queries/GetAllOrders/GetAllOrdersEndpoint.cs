using Devlivery.Shared.Presentation.Models;
using Devlivery.Shared.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.Features.Orders.Queries.GetAllOrders;

public static class GetAllOrdersEndpoint
{
    public sealed record Request(DateTime? Start, DateTime? End);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("", Handle)
            .Produces<ApiResponse<List<GetAllOrdersResponse>>>()
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Results<Ok<ApiResponse<List<GetAllOrdersResponse>>>, BadRequest<ProblemDetails>>> Handle(
        DateTime? start,
        DateTime? end,
        string? paymentMethod,
        GetAllOrdersHandler handler,
        CancellationToken ct)
    {
        var query = new GetAllOrdersQuery(start, end, paymentMethod);
        var result = await handler.HandleAsync(query, ct);

        return result.IsSuccess
            ? result.ToOk("Orders retrieved successfully")
            : result.ToBadRequestProblem();
    }
}