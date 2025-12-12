using Devlivery.Shared.Extensions;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.Features.Products.Commands.UpdateProduct;

public static class UpdateProductEndpoint
{
    internal sealed record Request(
        string Name,
        string Description,
        decimal Price,
        string Category,
        bool Available);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("{id:guid}", Handle)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<NoContent, ValidationProblem, NotFound<ProblemDetails>>> Handle(
        Guid id,
        Request request,
        ISender sender,
        IValidator<UpdateProductCommand> validator,
        CancellationToken ct)
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Description,
            request.Price,
            request.Category,
            request.Available);

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