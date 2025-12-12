using Devlivery.Features.Establishments.Domain;
using Devlivery.Features.Products.Domain;
using Devlivery.Features.Users.Domain;
using Devlivery.Shared.Identity.Users.Models;
using Devlivery.Shared.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.Shared.Persistence.Seeder;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        if (await db.Users.AnyAsync())
            return;

        var establishment = new Establishment(
            tradeName: "Pizza Devlivery",
            isActive: true
        );
        
        db.Establishments.Add(establishment);

        // Seed User
        var user = new User(
            name: "Atendente",
            email: "admin@pizza.com",
            establishmentId: establishment.Id
        );

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
            new(
                name: "Pizza Margherita",
                description: "Molho de tomate, mussarela e manjericão",
                price: 35.00m,
                category: "Pizza",
                available: true,
                establishmentId: establishment.Id
            ),
            new(
                name: "Pizza Calabresa",
                description: "Molho de tomate, mussarela, calabresa e cebola",
                price: 38.00m,
                category: "Pizza",
                available: true,
                establishmentId: establishment.Id
            ),
            new(
                name: "Refrigerante 2L",
                description: "Coca-Cola, Guaraná ou Fanta",
                price: 10.00m,
                category: "Bebida",
                available: true,
                establishmentId: establishment.Id
            )
        };

        db.Products.AddRange(products);
        await db.SaveChangesAsync();
    }
}