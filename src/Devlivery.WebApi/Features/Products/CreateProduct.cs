using Devlivery.WebApi.Features.Products.Domain;
using Devlivery.WebApi.Shared.Infrastructure.Database.Context;
using FluentValidation;

namespace Devlivery.WebApi.Features.Products;

public static class CreateProduct
{
    public record Request(
        string Name,
        string Description,
        decimal Price,
        string Category,
        bool Available);

    public record Response(
        Guid Id,
        string Name,
        string Description,
        decimal Price,
        string Category,
        bool Available,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
            RuleFor(x => x.Price).GreaterThan(0);
            RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        }
    }

    public static async Task<IResult> Handle(
        Request request,
        ApplicationDbContext db,
        IValidator<Request> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var now = DateTime.UtcNow;
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Category = request.Category,
            Available = request.Available,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();

        var response = new Response(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Category,
            product.Available,
            product.CreatedAt,
            product.UpdatedAt);

        return Results.Created($"/api/products/{product.Id}", response);
    }
}
