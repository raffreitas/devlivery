using Devlivery.Features.CashRegister.DTOs;
using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessionById;

public static class GetCashSessionByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("{id:guid}", Handle)
            .Produces<ApiResponse<CashSessionResponse>>()
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Results<Ok<ApiResponse<CashSessionResponse>>, ValidationProblem, NotFound<ProblemDetails>>> Handle(
        Guid id,
        IValidator<GetCashSessionByIdQuery> validator,
        GetCashSessionByIdHandler handler,
        CancellationToken ct)
    {
        var query = new GetCashSessionByIdQuery(id);

        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }

        var result = await handler.HandleAsync(query, ct);

        if (!result.IsSuccess && result.Errors.Any(e => e.Metadata.ContainsKey("NotFound")))
        {
            return result.ToNotFoundProblem();
        }

        return result.IsSuccess
            ? result.ToOk("Caixa recuperado com sucesso")
            : result.ToNotFoundProblem();
    }
}
