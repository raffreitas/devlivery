using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessions;

public static class GetCashSessionsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("sessions", Handle)
            .Produces<ApiResponse<List<GetCashSessionsResponse>>>()
            .Produces<ApiResponse<List<GetCashSessionsResponse>>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Results<Ok<ApiResponse<List<GetCashSessionsResponse>>>, BadRequest<ApiResponse<List<GetCashSessionsResponse>>>>> Handle(
        DateTime? start,
        DateTime? end,
        string? status,
        GetCashSessionsHandler handler,
        CancellationToken ct)
    {
        var query = new GetCashSessionsQuery(start, end, status);
        var result = await handler.HandleAsync(query, ct);

        return result.IsSuccess
            ? result.ToOk()
            : result.ToBadRequest();
    }
}