using Devlivery.Shared.Infrastructure.WebServer.Models;
using Mediator;

namespace Devlivery.Features.CashRegister.Queries.GetActiveCashSession;

public static class GetActiveCashSessionEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("sessions/active", Handle)
            .Produces<ApiResponse<GetActiveCashSessionResponse>>()
            .Produces<ApiResponse<GetActiveCashSessionResponse>>(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> Handle(ISender sender, CancellationToken ct)
    {
        var query = new GetActiveCashSessionQuery();
        var result = await sender.Send(query, ct);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.NoContent();
    }
}