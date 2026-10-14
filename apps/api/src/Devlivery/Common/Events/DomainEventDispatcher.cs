using System.Collections.Concurrent;

using Devlivery.Domain.SeedWork;

namespace Devlivery.Common.Events;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}

public sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    private static readonly ConcurrentDictionary<Type, EventDispatcher> Dispatchers = new();

    public Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var dispatcher = Dispatchers.GetOrAdd(
            domainEvent.GetType(),
            eventType => (EventDispatcher)Activator.CreateInstance(
                typeof(EventDispatcher<>).MakeGenericType(eventType))!);

        return dispatcher.DispatchAsync(domainEvent, serviceProvider, cancellationToken);
    }

    // Bridges the runtime event type to the generic handler interface without reflection on every call.
    private abstract class EventDispatcher
    {
        public abstract Task DispatchAsync(
            IDomainEvent domainEvent,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken);
    }

    private sealed class EventDispatcher<TEvent> : EventDispatcher where TEvent : IDomainEvent
    {
        public override async Task DispatchAsync(
            IDomainEvent domainEvent,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken)
        {
            var handlers = serviceProvider.GetServices<IDomainEventHandler<TEvent>>();

            foreach (var handler in handlers)
            {
                await handler.HandleAsync((TEvent)domainEvent, cancellationToken);
            }
        }
    }
}