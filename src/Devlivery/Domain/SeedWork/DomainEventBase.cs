namespace Devlivery.Domain.SeedWork;

/// <summary>
/// Base class for domain events with common properties.
/// </summary>
public abstract record DomainEventBase : IDomainEvent
{
    public abstract Guid EstablishmentId { get; init; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}