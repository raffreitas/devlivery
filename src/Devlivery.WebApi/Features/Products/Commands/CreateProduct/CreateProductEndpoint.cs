using FluentValidation;

namespace Devlivery.WebApi.Features.Products.Commands.CreateProduct;

public static class CreateProductEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("", async (
            IValidator<CreateProductCommand> validator,
            CreateProductCommand request,
            CreateProductHandler handler,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var result = await handler.HandleAsync(request, ct);
            
            if (result.IsFailed)
            {
                return Results.Problem(result.Errors[0].Message);
            }

            return Results.Created($"/api/products/{result.Value.Id}", result.Value);
        });
    }
}
