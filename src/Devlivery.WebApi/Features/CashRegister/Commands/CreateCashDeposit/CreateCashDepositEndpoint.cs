using Devlivery.WebApi.Features.CashRegister.DTOs;
using Devlivery.WebApi.Shared.Extensions;
using Devlivery.WebApi.Shared.Presentation.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.WebApi.Features.CashRegister.Commands.CreateCashDeposit;

public static class CreateCashDepositEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("{cashSessionId:guid}/deposits", Handle)
            .Produces<ApiResponse<CashDepositResponse>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Created<ApiResponse<CashDepositResponse>>, ValidationProblem,
        BadRequest<ProblemDetails>, NotFound<ProblemDetails>>> Handle(
        Guid cashSessionId,
        CreateCashDepositCommand request,
        IValidator<CreateCashDepositCommand> validator,
        CreateCashDepositHandler handler,
        CancellationToken ct)
    {
        // Ensure the command has the correct cashSessionId
        var commandWithSessionId = request with { CashSessionId = cashSessionId };

        var validationResult = await validator.ValidateAsync(commandWithSessionId, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }

        var result = await handler.HandleAsync(commandWithSessionId, ct);

        if (!result.IsSuccess)
        {
            // Check if it's a NotFound error
            if (result.Errors.OfType<Devlivery.WebApi.Shared.Errors.NotFoundError>().Any())
            {
                return result.ToNotFoundProblem();
            }

            return result.ToBadRequestProblem();
        }

        return result.ToCreated($"/api/cash-sessions/{cashSessionId}/deposits/{result.Value.Id}",
            "Aporte adicionado com sucesso");
    }
}