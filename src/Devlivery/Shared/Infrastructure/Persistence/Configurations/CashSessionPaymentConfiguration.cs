using Devlivery.Features.CashRegister.Domain.Entities;
using Devlivery.Features.Establishments.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devlivery.Shared.Infrastructure.Persistence.Configurations;

public sealed class CashSessionPaymentConfiguration : IEntityTypeConfiguration<CashSessionPayment>
{
    public void Configure(EntityTypeBuilder<CashSessionPayment> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.CashSessionId).IsRequired();
        builder.Property(p => p.OrderPaymentId).IsRequired();
        builder.Property(p => p.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(p => p.PaymentMethod).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(p => p.RecordedAt).IsRequired();
        builder.Property(x => x.RelatedOrderId).IsRequired();
        builder.Property(p => p.EntryType).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(p => p.Reason).HasMaxLength(500).IsRequired(false);

        builder.HasOne<Establishment>()
            .WithMany()
            .HasForeignKey(e => e.EstablishmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.EstablishmentId);
        builder.HasIndex(p => p.CashSessionId);
        builder.HasIndex(p => p.OrderPaymentId);
        builder.HasIndex(p => p.RelatedOrderId);
    }
}