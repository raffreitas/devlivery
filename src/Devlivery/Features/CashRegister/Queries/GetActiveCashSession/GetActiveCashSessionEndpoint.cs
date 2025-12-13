using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.CashRegister.Queries.GetActiveCashSession;

public static class GetActiveCashSessionEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("active", Handle)
            .Produces<ApiResponse<GetActiveCashSessionResponse>>()
            .Produces<ApiResponse<GetActiveCashSessionResponse>>(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<ApiResponse<GetActiveCashSessionResponse>>, NotFound<ApiResponse<GetActiveCashSessionResponse>>>> Handle(
        GetActiveCashSessionHandler handler,
        CancellationToken ct)
    {
        var query = new GetActiveCashSessionQuery();
        var result = await handler.HandleAsync(query, ct);

        return result.IsSuccess
            ? result.ToOk()
            : result.ToNotFound();
    }
}