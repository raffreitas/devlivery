using Devlivery.Features.Establishments.Domain;
using Devlivery.Features.Orders.Domain;
using Devlivery.Shared.SeedWork;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devlivery.Shared.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.ComplexProperty(o => o.Customer, customer =>
        {
            customer.Property(c => c.Name)
                .HasColumnName("customer_name")
                .IsRequired()
                .HasMaxLength(200);

            customer.Property(c => c.Phone)
                .HasColumnName("customer_phone")
                .IsRequired(false)
                .HasMaxLength(20)
                .HasConversion(
                    v => v != null ? v.Number : null,
                    v => !string.IsNullOrWhiteSpace(v) ? new PhoneNumber(v) : null
                );
        });

        builder.ComplexProperty(o => o.DeliveryAddress, address =>
        {
            address.Property(a => a.FullAddress)
                .HasColumnName("delivery_address")
                .IsRequired()
                .HasMaxLength(500);

            address.Property(a => a.Reference)
                .HasColumnName("delivery_reference")
                .IsRequired(false)
                .HasMaxLength(200);
        });

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