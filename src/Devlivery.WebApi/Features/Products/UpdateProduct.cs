using Devlivery.WebApi.Shared.Infrastructure.Database.Context;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.Products;

public static class UpdateProduct
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
        Guid id,
        Request request,
        ApplicationDbContext db,
        IValidator<Request> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
        {
            return Results.NotFound();
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Category = request.Category;
        product.Available = request.Available;
        product.UpdatedAt = DateTime.UtcNow;

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

        return Results.Ok(response);
    }
}
