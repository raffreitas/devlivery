using Devlivery.Features.CashRegister.DTOs;
using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessions;

public static class GetCashSessionsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("", Handle)
            .Produces<ApiResponse<List<CashSessionResponse>>>()
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Results<Ok<ApiResponse<List<CashSessionResponse>>>, ValidationProblem, BadRequest<ProblemDetails>>> Handle(
        DateTime? start,
        DateTime? end,
        string? status,
        IValidator<GetCashSessionsQuery> validator,
        GetCashSessionsHandler handler,
        CancellationToken ct)
    {
        var query = new GetCashSessionsQuery(start, end, status);

        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }

        var result = await handler.HandleAsync(query, ct);

        return result.IsSuccess
            ? result.ToOk("Caixas recuperados com sucesso")
            : result.ToBadRequestProblem();
    }
}
