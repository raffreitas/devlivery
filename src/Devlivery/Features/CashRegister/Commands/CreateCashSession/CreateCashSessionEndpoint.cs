using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.Features.CashRegister.Commands.CreateCashSession;

public static class CreateCashSessionEndpoint
{
    internal sealed record Request(
        Guid AttendantId,
        string AttendantName,
        decimal OpeningAmount,
        string? Notes);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("", Handle)
            .Produces<ApiResponse<CreateCashSessionResponse>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Results<Created<ApiResponse<CreateCashSessionResponse>>, ValidationProblem,
        BadRequest<ProblemDetails>>> Handle(
        Request request,
        ISender sender,
        IValidator<CreateCashSessionCommand> validator,
        CancellationToken ct)
    {
        var command = new CreateCashSessionCommand(request.AttendantId, request.AttendantName, request.OpeningAmount, request.Notes);

        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }

        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? result.ToCreated($"/api/cash-sessions/{result.Value.Id}")
            : result.ToBadRequestProblem();
    }
}