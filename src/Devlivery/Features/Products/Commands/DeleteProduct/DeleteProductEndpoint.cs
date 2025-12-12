using Devlivery.Shared.Extensions;

using FluentValidation;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.Features.Products.Commands.DeleteProduct;

public static class DeleteProductEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("{id:guid}", Handle)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<NoContent, ValidationProblem, NotFound<ProblemDetails>>> Handle(
        Guid id,
        ISender sender,
        IValidator<DeleteProductCommand> validator,
        CancellationToken ct)
    {
        var command = new DeleteProductCommand(id);

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