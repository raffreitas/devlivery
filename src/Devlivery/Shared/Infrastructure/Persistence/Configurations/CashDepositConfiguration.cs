using Devlivery.Features.CashRegister.Domain;
using Devlivery.Features.Establishments.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devlivery.Shared.Infrastructure.Persistence.Configurations;

public sealed class CashDepositConfiguration : IEntityTypeConfiguration<CashDeposit>
{
    public void Configure(EntityTypeBuilder<CashDeposit> builder)
    {
        builder.ToTable("cash_deposits");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.CashSessionId).IsRequired();
        builder.Property(x => x.EstablishmentId).IsRequired();
        builder.Property(x => x.AttendantId).IsRequired();
        builder.Property(x => x.AttendantName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.DepositedAt).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000).IsRequired(false);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        // Relationships
        builder.HasOne<Establishment>()
            .WithMany()
            .HasForeignKey(x => x.EstablishmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.CashSessionId);
        builder.HasIndex(x => x.EstablishmentId);
        builder.HasIndex(x => x.DepositedAt);
    }
}