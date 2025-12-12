using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;
using Devlivery.Shared.SeedWork.Errors;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.Features.CashRegister.Commands.CloseCashSession;

public static class CloseCashSessionEndpoint
{
    internal sealed record Request(decimal ClosingAmount, string? Notes);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("{id:guid}/close", Handle)
            .Produces<ApiResponse<CloseCashSessionResponse>>()
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Results<Ok<ApiResponse<CloseCashSessionResponse>>, ValidationProblem, NotFound<ProblemDetails>,
        BadRequest<ProblemDetails>>> Handle(
        Guid id,
        Request request,
        ISender sender,
        IValidator<CloseCashSessionCommand> validator,
        CancellationToken ct)
    {
        var command = new CloseCashSessionCommand(id, request.ClosingAmount, request.Notes);

        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }

        var result = await sender.Send(command, ct);

        if (!result.IsSuccess && result.HasError<NotFoundError>())
        {
            return result.ToNotFoundProblem();
        }

        return result.IsSuccess
            ? result.ToOk()
            : result.ToBadRequestProblem();
    }
}