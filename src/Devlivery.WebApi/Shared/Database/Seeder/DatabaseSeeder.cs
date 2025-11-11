using Devlivery.WebApi.Features.Products.Domain;
using Devlivery.WebApi.Features.Users.Domain;
using Devlivery.WebApi.Shared.Database.Context;
using Devlivery.WebApi.Shared.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Shared.Database.Seeder;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        if (await db.Users.AnyAsync())
            return;

        // Seed User
        var user = new User(
            name: "Atendente",
            email: "admin@pizza.com"
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
                available: true
            ),
            new(
                name: "Pizza Calabresa",
                description: "Molho de tomate, mussarela, calabresa e cebola",
                price: 38.00m,
                category: "Pizza",
                available: true
            ),
            new(
                name: "Refrigerante 2L",
                description: "Coca-Cola, Guaraná ou Fanta",
                price: 10.00m,
                category: "Bebida",
                available: true
            )
        };

        db.Products.AddRange(products);
        await db.SaveChangesAsync();
    }
}