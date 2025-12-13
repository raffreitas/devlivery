using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.Orders.Queries.GetOrderById;

public static class GetOrderByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("{id:guid}", Handle)
            .Produces<ApiResponse<GetOrderByIdResponse>>()
            .Produces<ApiResponse<GetOrderByIdResponse>>(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<ApiResponse<GetOrderByIdResponse>>, NotFound<ApiResponse<GetOrderByIdResponse>>>> Handle(
        Guid id,
        GetOrderByIdHandler handler,
        CancellationToken ct)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await handler.HandleAsync(query, ct);

        return result.IsSuccess
            ? result.ToOk()
            : result.ToNotFound();
    }
}