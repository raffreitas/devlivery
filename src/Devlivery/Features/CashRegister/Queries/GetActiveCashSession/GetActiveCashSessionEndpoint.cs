using Devlivery.Infrastructure.WebServer.Extensions;
using Devlivery.Infrastructure.WebServer.Models;

using Mediator;

namespace Devlivery.Features.CashRegister.Queries.GetActiveCashSession;

public static class GetActiveCashSessionEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("sessions/active", Handle)
            .Produces<ApiResponse<GetActiveCashSessionResponse>>()
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(ISender sender, CancellationToken ct)
    {
        var query = new GetActiveCashSessionQuery();
        var result = await sender.Send(query, ct);
        return result.ToApiResult();
    }
}