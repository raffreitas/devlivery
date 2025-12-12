using Devlivery.Shared.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.Features.Orders.Commands.UpdateOrder;

public static class UpdateOrderEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("{id:guid}", Handle)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Results<NoContent, ValidationProblem, NotFound<ProblemDetails>,
        BadRequest<ProblemDetails>>> Handle(
        Guid id,
        UpdateOrderCommand request,
        IValidator<UpdateOrderCommand> validator,
        UpdateOrderHandler handler,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return validationResult.ToValidationProblem();

        var result = await handler.HandleAsync(request, ct);

        if (result.IsSuccess)
            return result.ToNoContent();

        var errorMessage = result.Errors[0]?.Message ?? string.Empty;

        return errorMessage == "Pedido não encontrado"
            ? result.ToNotFoundProblem()
            : result.ToBadRequestProblem();
    }
}