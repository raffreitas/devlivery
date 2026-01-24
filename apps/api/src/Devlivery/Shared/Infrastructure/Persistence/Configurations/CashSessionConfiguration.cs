using Devlivery.Features.CashRegister.Domain;
using Devlivery.Features.CashRegister.Domain.Entities;
using Devlivery.Features.Establishments.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devlivery.Shared.Infrastructure.Persistence.Configurations;

public sealed class CashSessionConfiguration : IEntityTypeConfiguration<CashSession>
{
    public void Configure(EntityTypeBuilder<CashSession> builder)
    {
        builder.ToTable("cash_sessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.EstablishmentId).IsRequired();
        builder.Property(x => x.AttendantId).IsRequired();
        builder.Property(x => x.AttendantName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.OpeningAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.ClosingAmount).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000).IsRequired(false);

        builder.Property(x => x.StartAt).IsRequired();
        builder.Property(x => x.EndAt).IsRequired(false);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
        builder.Property<byte[]>("RowVersion").IsRowVersion().HasColumnName("row_version");

        builder.HasOne<Establishment>()
            .WithMany()
            .HasForeignKey(x => x.EstablishmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany<CashSessionMovement>(x => x.Movements)
            .WithOne()
            .HasForeignKey(d => d.CashSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(a => a.Movements)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.EstablishmentId);
        builder.HasIndex(x => x.Status);

        builder.Ignore(x => x.ExpectedCashAmount);
        builder.Ignore(x => x.TotalOrders);
        builder.Ignore(x => x.TotalRevenue);
    }
}