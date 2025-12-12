using Devlivery.Shared.Extensions;

using FluentValidation;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.Features.Orders.Commands.UpdateOrderStatus;

public static class UpdateOrderStatusEndpoint
{
    internal sealed record Request(string Status);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("{id:guid}/status", Handle)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<NoContent, ValidationProblem, NotFound<ProblemDetails>>> Handle(
        Guid id,
        Request request,
        ISender sender,
        IValidator<UpdateOrderStatusCommand> validator,
        CancellationToken ct)
    {
        var command = new UpdateOrderStatusCommand(id, request.Status);

        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }

        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? result.ToNoContent()
            : result.ToNotFoundProblem();
    }
}