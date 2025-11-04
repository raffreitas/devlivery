using FluentValidation;

namespace Devlivery.WebApi.Features.Products.Queries.GetProductById;

public static class GetProductByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("{id:guid}", async (
            Guid id,
            IValidator<GetProductByIdQuery> validator,
            GetProductByIdHandler handler,
            CancellationToken ct) =>
        {
            var query = new GetProductByIdQuery(id);

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