using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Infrastructure.Http.Models;

using Mediator;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessions;

public static class GetCashSessionsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("sessions", Handle)
            .Produces<ApiResponse<GetCashSessionsResponse[]>>()
            .Produces<ApiResponse<GetCashSessionsResponse[]>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(DateTime? start, DateTime? end, CashSessionStatus? status, ISender sender,
        CancellationToken ct)
    {
        var query = new GetCashSessionsQuery(start, end, status);
        var result = await sender.Send(query, ct);

        return TypedResults.Ok(ApiResponse<GetCashSessionsResponse[]>.Success(result));
    }
}