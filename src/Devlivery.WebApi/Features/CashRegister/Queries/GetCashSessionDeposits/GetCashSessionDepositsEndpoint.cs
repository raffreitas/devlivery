using Devlivery.WebApi.Features.CashRegister.DTOs;
using Devlivery.WebApi.Shared.Extensions;
using Devlivery.WebApi.Shared.Presentation.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.WebApi.Features.CashRegister.Queries.GetCashSessionDeposits;

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
