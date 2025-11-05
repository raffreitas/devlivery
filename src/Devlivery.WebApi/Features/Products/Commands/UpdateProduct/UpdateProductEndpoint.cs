using Devlivery.WebApi.Shared.Extensions;
using Devlivery.WebApi.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.WebApi.Features.Products.Commands.UpdateProduct;

public static class UpdateProductEndpoint
{
    public record Request(
        string Name,
        string Description,
        decimal Price,
        string Category,
        bool Available);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("{id:guid}", Handle)
            .Produces<ApiResponse<UpdateProductResponse>>()
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<ApiResponse<UpdateProductResponse>>, ValidationProblem, NotFound<ProblemDetails>>> Handle(
        Guid id,
        Request request,
        IValidator<UpdateProductCommand> validator,
        UpdateProductHandler handler,
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

        var result = await handler.HandleAsync(command, ct);

        return result.IsSuccess
            ? result.ToOk("Product updated successfully")
            : result.ToNotFoundProblem();
    }
}
