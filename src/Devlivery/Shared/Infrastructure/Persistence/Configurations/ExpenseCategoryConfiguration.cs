using Devlivery.Features.Establishments.Domain;
using Devlivery.Features.Expenses.Domain.Aggregates.Categories;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devlivery.Shared.Infrastructure.Persistence.Configurations;

public sealed class ExpenseCategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("expense_categories");

        builder.HasKey(e => e.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.IsActive)
            .IsRequired();

        builder.Property(x => x.EstablishmentId)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .IsRequired();

        builder.HasOne<Establishment>()
            .WithMany()
            .HasForeignKey(e => e.EstablishmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Category>()
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.EstablishmentId);
        builder.HasIndex(e => e.IsActive);

        // Seed data - Categorias padrão (IDs fixos para consistência)
        // TODO: Alterar para insert na migration fazendo o select em todos os estaabelecimentos disponíveis
        //  SeedDefaultCategories(builder);
    }

    //  private static void SeedDefaultCategories(EntityTypeBuilder<Category> builder)
    //  {
    // Nota: EstablishmentId será o ID do primeiro estabelecimento criado no sistema
    // Em produção, estas categorias devem ser copiadas para cada novo estabelecimento
    //       var defaultEstablishmentId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    //       var companyExpenseId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    //       var supplierId = Guid.Parse("10000000-0000-0000-0000-000000000002");

    // //        builder.HasData(
    //  new
    //  {
    //      Id = companyExpenseId,
    //      Name = "Despesas da Empresa",
    //      CategoryType = Features.Expenses.Domain.Enums.ExpenseCategory.CompanyExpense,
    //      RequiresSupplier = false,
    //      IsActive = true,
    //      EstablishmentId = defaultEstablishmentId,
    //      CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
    //      UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    //  },
    //  new
    //  {
    //      Id = supplierId,
    //      Name = "Fornecedor",
    //      CategoryType = Features.Expenses.Domain.Enums.ExpenseCategory.Supplier,
    //      RequiresSupplier = true,
    //      IsActive = true,
    //      EstablishmentId = defaultEstablishmentId,
    //      CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
    //      UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    //  }
    // );
}