using Devlivery.Domain.Aggregates.CashRegister.Entities;
using Devlivery.Domain.Aggregates.Establishments;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devlivery.Infrastructure.Persistence.Configurations;

public sealed class CashSessionMovementConfiguration : IEntityTypeConfiguration<CashSessionMovement>
{
    public void Configure(EntityTypeBuilder<CashSessionMovement> builder)
    {
        builder.ToTable("cash_session_movements");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.CashSessionId).IsRequired();
        builder.Property(x => x.EstablishmentId).IsRequired();
        builder.Property(x => x.EntryType).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(50).IsRequired(false);
        builder.Property(x => x.RelatedOrderId).IsRequired(false);
        builder.Property(x => x.OrderPaymentId).IsRequired(false);
        builder.Property(x => x.Reason).HasMaxLength(1000).IsRequired(false);
        builder.Property(x => x.CreatedBy).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property<byte[]>("RowVersion")
            .IsRowVersion()
            .HasColumnName("row_version");

        builder.HasOne<Establishment>()
            .WithMany()
            .HasForeignKey(x => x.EstablishmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.EstablishmentId);
        builder.HasIndex(x => x.CashSessionId);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.RelatedOrderId);

        builder.HasIndex(x => new { x.OrderPaymentId, x.CashSessionId, x.EntryType })
            .HasDatabaseName("idx_cash_session_movements_unique_payment")
            .IsUnique()
            .HasFilter("""
                       "order_payment_id" IS NOT NULL AND "entry_type" = 'Payment'
                       """);
    }
}