using Devlivery.WebApi.Features.Establishments.Domain;
using Devlivery.WebApi.Features.Orders.Domain;
using Devlivery.WebApi.Features.Products.Domain;
using Devlivery.WebApi.Features.Users.Domain;
using Devlivery.WebApi.Shared.Database.Configurations;
using Devlivery.WebApi.Shared.Database.Extensions;
using Devlivery.WebApi.Shared.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Shared.Database.Context;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Establishment> Establishments => Set<Establishment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());

        modelBuilder.UseUtcDateTimeConverter();
    }
}