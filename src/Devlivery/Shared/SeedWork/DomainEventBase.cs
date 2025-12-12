namespace Devlivery.Shared.SeedWork;

/// <summary>
/// Base class for domain events with common properties.
/// </summary>
public abstract record DomainEventBase : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}