using Devlivery.Features.Establishments.Domain;
using Devlivery.Features.Orders.Domain;
using Devlivery.Features.Products.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devlivery.Shared.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(e => e.Quantity).IsRequired();
        builder.Property(e => e.UnitPrice).HasPrecision(18, 2)
            .IsRequired();
        builder.Property(e => e.Notes).HasMaxLength(500);
        builder.Property(x => x.EstablishmentId).IsRequired();

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Establishment>()
            .WithMany()
            .HasForeignKey(e => e.EstablishmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.EstablishmentId);
    }
}