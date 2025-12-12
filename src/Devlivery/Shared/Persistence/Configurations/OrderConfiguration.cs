using Devlivery.Features.Establishments.Domain;
using Devlivery.Features.Orders.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devlivery.Shared.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(e => e.CustomerName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.CustomerPhone).IsRequired(false).HasMaxLength(20);
        builder.Property(e => e.DeliveryAddress).IsRequired().HasMaxLength(500);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20).HasConversion<string>();
        builder.Property(e => e.Total).HasPrecision(18, 2);
        builder.Property(e => e.DeliveryFee).HasPrecision(18, 2);
        builder.Property(e => e.PaymentMethod).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.EstablishmentId).IsRequired();
        builder.Property(e => e.Notes).HasMaxLength(500).IsRequired(false);

        builder.HasMany(e => e.Items)
            .WithOne()
            .HasForeignKey("order_id").IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(a => a.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne<Establishment>()
            .WithMany()
            .HasForeignKey(e => e.EstablishmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.EstablishmentId);
    }
}