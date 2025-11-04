using FluentValidation;

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
        app.MapPut("{id:guid}", async (
            Guid id,
            Request request,
            IValidator<UpdateProductCommand> validator,
            UpdateProductHandler handler,
            CancellationToken ct) =>
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
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var result = await handler.HandleAsync(command, ct);
            
            if (result.IsFailed)
            {
                return Results.NotFound(new { message = result.Errors[0].Message });
            }

            return Results.Ok(result.Value);
        });
    }
}
