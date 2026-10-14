using Devlivery.Common.Events;
using Devlivery.Domain.SeedWork;
using Devlivery.Infrastructure.Persistence.Context;

namespace Devlivery.Infrastructure.Persistence;

/// <summary>
/// Dispatches domain events before saving, so that changes made by event handlers
/// are persisted atomically together with the changes that raised the events.
/// </summary>
public sealed class UnitOfWork(ApplicationDbContext dbContext, IDomainEventDispatcher dispatcher) : IUnitOfWork
{
    // Guards against handlers that keep raising events for each other.
    private const int MaxDispatchRounds = 10;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(cancellationToken);
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        for (var round = 0; round < MaxDispatchRounds; round++)
        {
            var domainEvents = CollectDomainEvents();
            if (domainEvents.Count == 0)
                return;

            foreach (var domainEvent in domainEvents)
            {
                await dispatcher.DispatchAsync(domainEvent, cancellationToken);
            }
        }

        throw new InvalidOperationException(
            $"Domain events were still being raised after {MaxDispatchRounds} dispatch rounds.");
    }

    private List<IDomainEvent> CollectDomainEvents()
    {
        var entities = dbContext.ChangeTracker
            .Entries<Entity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count != 0)
            .ToList();

        var domainEvents = entities.SelectMany(e => e.DomainEvents).ToList();
        entities.ForEach(e => e.ClearDomainEvents());

        return domainEvents;
    }
}