using Devlivery.WebApi.Features.CashRegister.DTOs;
using Devlivery.WebApi.Shared.Extensions;
using Devlivery.WebApi.Shared.Presentation.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.WebApi.Features.CashRegister.Queries.GetActiveCashSession;

public static class GetActiveCashSessionEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("active", Handle)
            .Produces<ApiResponse<CashSessionResponse>>()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<ApiResponse<CashSessionResponse>>, NotFound<ProblemDetails>>> Handle(
        GetActiveCashSessionHandler handler,
        CancellationToken ct)
    {
        var query = new GetActiveCashSessionQuery();
        var result = await handler.HandleAsync(query, ct);

        return result.IsSuccess
            ? result.ToOk("Caixa recuperado com sucesso")
            : result.ToNotFoundProblem();
    }
}
