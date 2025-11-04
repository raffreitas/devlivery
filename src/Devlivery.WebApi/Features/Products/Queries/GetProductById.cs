using Devlivery.WebApi.Shared.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.Products;

public static class GetProductById
{
    public record Response(
        Guid Id,
        string Name,
        string Description,
        decimal Price,
        string Category,
        bool Available,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    public static async Task<IResult> Handle(Guid id, ApplicationDbContext db)
    {
        var product = await db.Products
            .Where(p => p.Id == id)
            .Select(p => new Response(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.Category,
                p.Available,
                p.CreatedAt,
                p.UpdatedAt))
            .FirstOrDefaultAsync();

        return product is null ? Results.NotFound() : Results.Ok(product);
    }
}
