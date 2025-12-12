using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessionDeposits;

public static class GetCashSessionDepositsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("{cashSessionId}/deposits", Handle)
            .Produces<ApiResponse<IEnumerable<GetCashSessionDepositsResponse>>>()
            .Produces<ApiResponse<IEnumerable<GetCashSessionDepositsResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<Ok<ApiResponse<IEnumerable<GetCashSessionDepositsResponse>>>> Handle(
        Guid cashSessionId,
        GetCashSessionDepositsHandler handler,
        CancellationToken ct)
    {
        var query = new GetCashSessionDepositsQuery(cashSessionId);
        var result = await handler.HandleAsync(query, ct);

        return result.ToOk();
    }
}