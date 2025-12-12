using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.Features.CashRegister.Commands.CreateCashSession;

public static class CreateCashSessionEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("", Handle)
            .Produces<ApiResponse<CreateCashSessionResponse>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Results<Created<ApiResponse<CreateCashSessionResponse>>, ValidationProblem,
        BadRequest<ProblemDetails>>> Handle(
        CreateCashSessionCommand request,
        IValidator<CreateCashSessionCommand> validator,
        CreateCashSessionHandler handler,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }

        var result = await handler.HandleAsync(request, ct);

        return result.IsSuccess
            ? result.ToCreated($"/api/cash-sessions/{result.Value.Id}", "Caixa aberto com sucesso")
            : result.ToBadRequestProblem();
    }
}