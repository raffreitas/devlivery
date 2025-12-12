using Devlivery.Shared.SeedWork;

using Mediator;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Devlivery.Shared.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that dispatches domain events when SaveChanges is called.
/// This ensures domain events are published within the same transaction as the database changes.
/// </summary>
public sealed class DispatchDomainEventsInterceptor(IMediator mediator) : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(eventData.Context, cancellationToken);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        DispatchDomainEventsAsync(eventData.Context).GetAwaiter().GetResult();
        return base.SavingChanges(eventData, result);
    }

    private async Task DispatchDomainEventsAsync(DbContext? context, CancellationToken cancellationToken = default)
    {
        if (context is null) return;

        var entitiesWithEvents = context.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // Clear events before publishing to avoid infinite loops
        entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

        // Publish all events
        foreach (var domainEvent in domainEvents)
        {
            await mediator.Publish(domainEvent, cancellationToken);
        }
    }
}