using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;
using Devlivery.Shared.SeedWork.Errors;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.Features.CashRegister.Commands.CreateCashDeposit;

public static class CreateCashDepositEndpoint
{
    internal sealed record Request(
        Guid AttendantId,
        string AttendantName,
        decimal Amount,
        string? Notes);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("{cashSessionId:guid}/deposits", Handle)
            .Produces<ApiResponse<CreateCashDepositResponse>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Created<ApiResponse<CreateCashDepositResponse>>, ValidationProblem,
        BadRequest<ProblemDetails>, NotFound<ProblemDetails>>> Handle(
        Guid cashSessionId,
        Request request,
        ISender sender,
        IValidator<CreateCashDepositCommand> validator,
        CancellationToken ct)
    {
        // Create command with the correct cashSessionId from URL
        var command = new CreateCashDepositCommand(
            cashSessionId,
            request.AttendantId,
            request.AttendantName,
            request.Amount,
            request.Notes);

        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }

        var result = await sender.Send(command, ct);

        if (!result.IsSuccess)
        {
            // Check if it's a NotFound error
            if (result.Errors.OfType<NotFoundError>().Any())
            {
                return result.ToNotFoundProblem();
            }

            return result.ToBadRequestProblem();
        }

        return result.ToCreated($"/api/cash-sessions/{cashSessionId}/deposits/{result.Value.Id}");
    }
}