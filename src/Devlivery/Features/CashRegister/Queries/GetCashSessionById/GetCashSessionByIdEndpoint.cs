using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessionById;

public static class GetCashSessionByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("{id:guid}", Handle)
            .Produces<ApiResponse<GetCashSessionByIdResponse>>()
            .Produces<ApiResponse<GetCashSessionByIdResponse>>(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<ApiResponse<GetCashSessionByIdResponse>>, NotFound<ApiResponse<GetCashSessionByIdResponse>>>> Handle(
        Guid id,
        GetCashSessionByIdHandler handler,
        CancellationToken ct)
    {
        var query = new GetCashSessionByIdQuery(id);
        var result = await handler.HandleAsync(query, ct);

        return result.IsSuccess
            ? result.ToOk()
            : result.ToNotFound();
    }
}