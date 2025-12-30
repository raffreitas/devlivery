using Devlivery.Shared.Infrastructure.WebServer.Models;
using Mediator;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessionDeposits;

public static class GetCashSessionDepositsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("sessions/{cashSessionId:guid}/deposits", Handle)
            .Produces<ApiResponse<IEnumerable<GetCashSessionDepositsResponse>>>();
    }

    private static async Task<IResult> Handle(Guid cashSessionId, ISender sender, CancellationToken ct)
    {
        var query = new GetCashSessionDepositsQuery(cashSessionId);
        var result = await sender.Send(query, ct);

        return TypedResults.Ok(result);
    }
}