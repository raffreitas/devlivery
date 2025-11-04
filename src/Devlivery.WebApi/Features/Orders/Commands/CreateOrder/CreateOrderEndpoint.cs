using FluentValidation;

namespace Devlivery.WebApi.Features.Orders.Commands.CreateOrder;

public static class CreateOrderEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("", async (
            IValidator<CreateOrderCommand> validator,
            CreateOrderCommand request,
            CreateOrderHandler handler,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var order = await handler.HandleAsync(request, ct);
            return Results.Created($"", order.Value);
        });
    }
}