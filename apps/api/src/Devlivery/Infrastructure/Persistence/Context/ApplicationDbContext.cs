using System.Reflection;

using Devlivery.Domain.Aggregates.CashRegister;
using Devlivery.Domain.Aggregates.CashRegister.Entities;
using Devlivery.Domain.Aggregates.Establishments;
using Devlivery.Domain.Aggregates.Expenses;
using Devlivery.Domain.Aggregates.Orders;
using Devlivery.Domain.Aggregates.Orders.Entities;
using Devlivery.Domain.Aggregates.Products;
using Devlivery.Domain.Aggregates.Users;
using Devlivery.Domain.SeedWork;
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

        ApplyTenantQueryFilters(modelBuilder);

        modelBuilder.UseUtcDateTimeConverter();
    }

    private Guid CurrentTenantId => tenantAccessor.Tenant.Id;

    private void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
    {
        var applyFilter = typeof(ApplicationDbContext).GetMethod(
            nameof(ApplyTenantQueryFilter),
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        var tenantScopedTypes = modelBuilder.Model.GetEntityTypes()
            .Select(entityType => entityType.ClrType)
            .Where(clrType => clrType.IsAssignableTo(typeof(ITenantScoped)))
            .ToList();

        foreach (var clrType in tenantScopedTypes)
            applyFilter.MakeGenericMethod(clrType).Invoke(this, [modelBuilder]);
    }

    private void ApplyTenantQueryFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantScoped =>
        modelBuilder.Entity<TEntity>().HasQueryFilter(entity => entity.EstablishmentId == CurrentTenantId);
}