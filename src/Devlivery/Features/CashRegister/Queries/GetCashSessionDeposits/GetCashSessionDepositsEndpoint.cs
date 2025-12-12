using Devlivery.Features.CashRegister.DTOs;
using Devlivery.Shared.Presentation.Models;
using Devlivery.Shared.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessionDeposits;

public static class GetCashSessionDepositsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("{cashSessionId}/deposits", Handle)
            .Produces<ApiResponse<IEnumerable<CashDepositResponse>>>()
            .Produces<ApiResponse<IEnumerable<CashDepositResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<Ok<ApiResponse<IEnumerable<CashDepositResponse>>>> Handle(
        Guid cashSessionId,
        GetCashSessionDepositsHandler handler,
        CancellationToken ct)
    {
        var query = new GetCashSessionDepositsQuery(cashSessionId);
        var result = await handler.HandleAsync(query, ct);

        return result.ToOk("Aportes recuperados com sucesso");
    }
}
