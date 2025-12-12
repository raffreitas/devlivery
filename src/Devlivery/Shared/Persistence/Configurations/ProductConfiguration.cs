using Devlivery.Features.Establishments.Domain;
using Devlivery.Features.Products.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devlivery.Shared.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(1000);
        builder.Property(e => e.Price).HasPrecision(18, 2);
        builder.Property(e => e.Category).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Available).IsRequired();
        builder.Property(x => x.EstablishmentId).IsRequired();

        builder.HasOne<Establishment>()
            .WithMany()
            .HasForeignKey(e => e.EstablishmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.EstablishmentId);
    }
}