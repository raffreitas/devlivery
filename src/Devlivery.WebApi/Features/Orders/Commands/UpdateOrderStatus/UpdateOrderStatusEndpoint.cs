using FluentValidation;

namespace Devlivery.WebApi.Features.Orders.Commands.UpdateOrderStatus;

public static class UpdateOrderStatusEndpoint
{
    public record Request(string Status);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("{id:guid}/status", async (
            Guid id,
            Request request,
            IValidator<UpdateOrderStatusCommand> validator,
            UpdateOrderStatusHandler handler,
            CancellationToken ct) =>
        {
            var command = new UpdateOrderStatusCommand(id, request.Status);
            
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
