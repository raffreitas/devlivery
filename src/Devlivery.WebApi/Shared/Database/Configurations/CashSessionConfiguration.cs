using System.Text.Json;
using Devlivery.WebApi.Features.CashRegister.Domain;
using Devlivery.WebApi.Features.Establishments.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devlivery.WebApi.Shared.Database.Configurations;

public sealed class CashSessionConfiguration : IEntityTypeConfiguration<CashSession>
{
    public void Configure(EntityTypeBuilder<CashSession> builder)
    {
        builder.ToTable("cash_sessions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.EstablishmentId).IsRequired();
        builder.Property(x => x.AttendantId).IsRequired();
        builder.Property(x => x.AttendantName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.OpeningAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.ClosingAmount).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000).IsRequired(false);
        builder.Property(x => x.TotalRevenue).HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(x => x.TotalOrders).HasDefaultValue(0);
        builder.Property(x => x.ExpectedCashAmount).HasPrecision(18, 2).IsRequired();

        builder.Property(x => x.PaymentBreakdown)
            .HasColumnName("payment_breakdown")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<PaymentBreakdownItem>>(v, (JsonSerializerOptions?)null) ??
                     new List<PaymentBreakdownItem>());

        builder.Property(x => x.StartAt).IsRequired();
        builder.Property(x => x.EndAt).IsRequired(false);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOne<Establishment>()
            .WithMany()
            .HasForeignKey(x => x.EstablishmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany<CashDeposit>(x => x.Deposits)
            .WithOne()
            .HasForeignKey(d => d.CashSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(a => a.Deposits)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.EstablishmentId);
        builder.HasIndex(x => x.Status);
    }
}