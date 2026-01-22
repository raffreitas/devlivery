using Mediator;

namespace Devlivery.Common.SeedWork;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something that happened in the domain that you want other parts of the same domain to be aware of.
/// </summary>
public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}