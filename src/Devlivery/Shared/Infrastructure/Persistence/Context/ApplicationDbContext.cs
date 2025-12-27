using Devlivery.Features.CashRegister.Domain;
using Devlivery.Features.Establishments.Domain;
using Devlivery.Features.Expenses.Domain.Aggregates.Categories;
using Devlivery.Features.Expenses.Domain.Aggregates.Expenses;
using Devlivery.Features.Orders.Domain;
using Devlivery.Features.Orders.Domain.Entities;
using Devlivery.Features.Products.Domain;
using Devlivery.Features.Users.Domain;
using Devlivery.Shared.Infrastructure.Persistence.Configurations;
using Devlivery.Shared.Infrastructure.Persistence.Extensions;
using Devlivery.Shared.Infrastructure.Tenancy;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Shared.Infrastructure.Persistence.Context;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ITenantAccessor tenantAccessor)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Establishment> Establishments => Set<Establishment>();
    public DbSet<CashSession> CashSessions => Set<CashSession>();
    public DbSet<CashDeposit> CashDeposits => Set<CashDeposit>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Category> ExpenseCategories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        modelBuilder.ApplyConfiguration(new CashSessionConfiguration());
        modelBuilder.ApplyConfiguration(new CashDepositConfiguration());
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
        modelBuilder.Entity<CashDeposit>().HasQueryFilter(x => x.EstablishmentId == tenantAccessor.Tenant.Id);
    }
}