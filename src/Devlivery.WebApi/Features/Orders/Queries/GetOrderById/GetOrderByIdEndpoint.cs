using FluentValidation;

namespace Devlivery.WebApi.Features.Orders.Queries.GetOrderById;

public static class GetOrderByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("{id:guid}", async (
            Guid id,
            IValidator<GetOrderByIdQuery> validator,
            GetOrderByIdHandler handler,
            CancellationToken ct) =>
        {
            var query = new GetOrderByIdQuery(id);

            var validationResult = await validator.ValidateAsync(query, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var result = await handler.HandleAsync(query, ct);

            return result.IsFailed
                ? Results.NotFound(new { message = result.Errors[0].Message })
                : Results.Ok(result.Value);
        });
    }
}