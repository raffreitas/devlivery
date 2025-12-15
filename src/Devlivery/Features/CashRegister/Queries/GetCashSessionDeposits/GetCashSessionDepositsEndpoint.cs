using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessionDeposits;

public static class GetCashSessionDepositsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("{cashSessionId:guid}/deposits", Handle)
            .Produces<ApiResponse<IEnumerable<GetCashSessionDepositsResponse>>>();
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