using Devlivery.WebApi.Shared.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.Products;

public static class DeleteProduct
{
    public static async Task<IResult> Handle(Guid id, ApplicationDbContext db)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
        {
            return Results.NotFound();
        }

        db.Products.Remove(product);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
