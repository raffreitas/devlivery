using Devlivery.Domain.Aggregates.CashRegister;
using Devlivery.Domain.Aggregates.CashRegister.Entities;
using Devlivery.Domain.Aggregates.Establishments;
using Devlivery.Domain.Aggregates.Expenses;
using Devlivery.Domain.Aggregates.Orders;
using Devlivery.Domain.Aggregates.Orders.Entities;
using Devlivery.Domain.Aggregates.Products;
using Devlivery.Features.Users.Domain;
using Devlivery.Infrastructure.Persistence.Configurations;
using Devlivery.Infrastructure.Persistence.Extensions;
using Devlivery.Infrastructure.Tenancy;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Infrastructure.Persistence.Context;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ITenantAccessor tenantAccessor)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderPayment> OrderPayments => Set<OrderPayment>();
    public DbSet<Establishment> Establishments => Set<Establishment>();
    public DbSet<CashSession> CashSessions => Set<CashSession>();
    public DbSet<CashSessionMovement> CashSessionMovements => Set<CashSessionMovement>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Category> ExpenseCategories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        modelBuilder.ApplyConfiguration(new OrderPaymentConfiguration());
        modelBuilder.ApplyConfiguration(new CashSessionConfiguration());
        modelBuilder.ApplyConfiguration(new CashSessionMovementConfiguration());
        modelBuilder.ApplyConfiguration(new ExpenseConfiguration());
        modelBuilder.ApplyConfiguration(new ExpenseCategoryConfiguration());

        ApplyQueryFilters(modelBuilder);

        modelBuilder.UseUtcDateTimeConverter();
    }

    private void ApplyQueryFilters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasQueryFilter(x => x.EstablishmentId == tenantAccessor.Tenant.Id);
        modelBuilder.Entity<Product>().HasQueryFilter(x => x.EstablishmentId == tenantAccessor.Tenant.Id);
        modelBuilder.Entity<Order>().HasQueryFilter(x => x.EstablishmentId == tenantAccessor.Tenant.Id);
        modelBuilder.Entity<Expense>().HasQueryFilter(x => x.EstablishmentId == tenantAccessor.Tenant.Id);
        modelBuilder.Entity<Category>().HasQueryFilter(x => x.EstablishmentId == tenantAccessor.Tenant.Id);
        modelBuilder.Entity<OrderItem>().HasQueryFilter(x => x.EstablishmentId == tenantAccessor.Tenant.Id);
        modelBuilder.Entity<CashSession>().HasQueryFilter(x => x.EstablishmentId == tenantAccessor.Tenant.Id);
        modelBuilder.Entity<CashSessionMovement>().HasQueryFilter(x => x.EstablishmentId == tenantAccessor.Tenant.Id);
    }
}