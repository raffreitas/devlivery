namespace Devlivery.Domain.SeedWork;

public abstract record DomainEventBase : IDomainEvent
{
    public abstract Guid EstablishmentId { get; init; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}