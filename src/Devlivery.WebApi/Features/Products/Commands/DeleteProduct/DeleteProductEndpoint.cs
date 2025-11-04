using FluentValidation;

namespace Devlivery.WebApi.Features.Products.Commands.DeleteProduct;

public static class DeleteProductEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("{id:guid}", async (
            Guid id,
            IValidator<DeleteProductCommand> validator,
            DeleteProductHandler handler,
            CancellationToken ct) =>
        {
            var command = new DeleteProductCommand(id);
            
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

            return Results.NoContent();
        });
    }
}
