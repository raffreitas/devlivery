using Devlivery.Domain.SeedWork;

namespace Devlivery.Common.Events;

/// <summary>
/// Handles a domain event within the current request scope.
/// Handlers only change aggregates; persistence is done by the UnitOfWork in a single save.
/// </summary>
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken);
}