using Devlivery.Domain.Aggregates.Establishments;
using Devlivery.Domain.SeedWork;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devlivery.Infrastructure.Persistence.Idempotency;

internal sealed class IdempotencyRecord : ITenantScoped
{
    public Guid EstablishmentId { get; init; }
    public required string Operation { get; init; }
    public Guid ScopeId { get; init; }
    public Guid RequestId { get; init; }
    public Guid ActorId { get; init; }
    public required string RequestHash { get; init; }
    public required string ResponseJson { get; init; }
}

internal sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    internal const string KeyName = "pk_api_idempotency_records";

    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("api_idempotency_records");
        builder.HasKey(x => new { x.EstablishmentId, x.Operation, x.ScopeId, x.RequestId }).HasName(KeyName);
        builder.Property(x => x.Operation).HasMaxLength(100);
        builder.Property(x => x.RequestHash).HasMaxLength(64);
        builder.Property(x => x.ResponseJson).HasColumnType("jsonb");

        builder.HasOne<Establishment>()
            .WithMany()
            .HasForeignKey(x => x.EstablishmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
