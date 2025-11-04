using Devlivery.WebApi.Shared.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.WebApi.Features.Products.Commands.DeleteProduct;

public static class DeleteProductEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("{id:guid}", Handle)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<NoContent, ValidationProblem, NotFound<ProblemDetails>>> Handle(
        Guid id,
        IValidator<DeleteProductCommand> validator,
        DeleteProductHandler handler,
        CancellationToken ct)
    {
        var command = new DeleteProductCommand(id);
        
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }

        var result = await handler.HandleAsync(command, ct);

        return result.IsSuccess
            ? result.ToNoContent()
            : result.ToNotFoundProblem();
    }
}
