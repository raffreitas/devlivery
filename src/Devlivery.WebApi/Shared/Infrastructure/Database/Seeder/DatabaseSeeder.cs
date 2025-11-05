using Devlivery.WebApi.Features.Products.Domain;
using Devlivery.WebApi.Features.Users.Domain;
using Devlivery.WebApi.Shared.Infrastructure.Database.Context;
using Devlivery.WebApi.Shared.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Shared.Infrastructure.Database.Seeder;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        if (await db.Users.AnyAsync())
            return;

        // Seed User
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Name = "Atendente",
            Email = "admin@pizza.com",
            CreatedAt = DateTime.UtcNow,
        };

        var identityResult = await userManager.CreateAsync(new ApplicationUser
        {
            UserId = user.Id,
            UserName = user.Email,
            Email = user.Email,
            EmailConfirmed = true,
        }, "ChangeIt123!");

        if (!identityResult.Succeeded)
            return;

        db.Users.Add(user);

        // Seed Products
        var products = new List<Product>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                Name = "Pizza Margherita",
                Description = "Molho de tomate, mussarela e manjericão",
                Price = 35.00m,
                Category = "Pizza",
                Available = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                Name = "Pizza Calabresa",
                Description = "Molho de tomate, mussarela, calabresa e cebola",
                Price = 38.00m,
                Category = "Pizza",
                Available = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                Name = "Refrigerante 2L",
                Description = "Coca-Cola, Guaraná ou Fanta",
                Price = 10.00m,
                Category = "Bebida",
                Available = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        db.Products.AddRange(products);
        await db.SaveChangesAsync();
    }
}