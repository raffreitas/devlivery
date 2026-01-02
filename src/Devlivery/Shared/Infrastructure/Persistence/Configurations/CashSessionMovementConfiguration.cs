using Devlivery.Features.CashRegister.Domain.Entities;
using Devlivery.Features.Establishments.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devlivery.Shared.Infrastructure.Persistence.Configurations;

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

        builder.HasOne<Establishment>()
            .WithMany()
            .HasForeignKey(x => x.EstablishmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.EstablishmentId);
        builder.HasIndex(x => x.CashSessionId);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.RelatedOrderId);
    }
}
