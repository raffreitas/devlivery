using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.Products.Queries.GetProductById;

public static class GetProductByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("{id:guid}", Handle)
            .Produces<ApiResponse<GetProductByIdResponse>>()
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<ApiResponse<GetProductByIdResponse>>, NotFound<ApiResponse<GetProductByIdResponse>>>> Handle(
        Guid id,
        GetProductByIdHandler handler,
        CancellationToken ct)
    {
        var query = new GetProductByIdQuery(id);
        var result = await handler.HandleAsync(query, ct);

        return result.IsSuccess
            ? result.ToOk()
            : result.ToNotFound();
    }
}