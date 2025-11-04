using Devlivery.WebApi.Shared.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.Products;

public static class GetAllProducts
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

    public static async Task<IResult> Handle(ApplicationDbContext db)
    {
        var products = await db.Products
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new Response(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.Category,
                p.Available,
                p.CreatedAt,
                p.UpdatedAt))
            .ToListAsync();

        return Results.Ok(products);
    }
}
