using Devlivery.WebApi.Shared.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.Orders;

public static class DeleteOrder
{
    public static async Task<IResult> Handle(Guid id, ApplicationDbContext db)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id);
        if (order is null)
        {
            return Results.NotFound();
        }

        db.Orders.Remove(order);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
