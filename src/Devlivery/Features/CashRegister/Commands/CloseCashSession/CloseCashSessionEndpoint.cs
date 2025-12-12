using Devlivery.Features.CashRegister.Queries.GetCashSessionById;
using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;
using Devlivery.Shared.SeedWork.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.Features.CashRegister.Commands.CloseCashSession;

public static class CloseCashSessionEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("{id:guid}/close", Handle)
            .Produces<ApiResponse<GetCashSessionByIdResponse>>()
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Results<Ok<ApiResponse<GetCashSessionByIdResponse>>, ValidationProblem, NotFound<ProblemDetails>,
        BadRequest<ProblemDetails>>> Handle(
        Guid id,
        CloseCashSessionCommand request,
        IValidator<CloseCashSessionCommand> validator,
        CloseCashSessionHandler handler,
        CancellationToken ct)
    {
        var command = request with { Id = id };

        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }

        var result = await handler.HandleAsync(command, ct);


        if (!result.IsSuccess && result.HasError<NotFoundError>())
        {
            return result.ToNotFoundProblem();
        }

        return result.IsSuccess
            ? result.ToOk()
            : result.ToBadRequestProblem();
    }
}