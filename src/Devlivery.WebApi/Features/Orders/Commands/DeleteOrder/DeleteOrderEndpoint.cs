using FluentValidation;

namespace Devlivery.WebApi.Features.Orders.Commands.DeleteOrder;

public static class DeleteOrderEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("{id:guid}", async (
            Guid id,
            IValidator<DeleteOrderCommand> validator,
            DeleteOrderHandler handler,
            CancellationToken ct) =>
        {
            var command = new DeleteOrderCommand(id);
            
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
