using Devlivery.Shared.SeedWork;

using Mediator;

namespace Devlivery.Shared.Infrastructure.Tenancy.Behaviors;

public sealed class DomainEventTenantBehavior<TNotification>(
    ITenantAccessor tenantAccessor,
    ILogger<DomainEventTenantBehavior<TNotification>> logger
) : IPipelineBehavior<TNotification, Unit> where TNotification : DomainEventBase
{
    public async ValueTask<Unit> Handle(
        TNotification message,
        MessageHandlerDelegate<TNotification, Unit> next,
        CancellationToken cancellationToken
    )
    {
        if (message.EstablishmentId != Guid.Empty)
        {
            var tenant = new Tenant(message.EstablishmentId);
            tenantAccessor.Register(tenant);

            logger.LogDebug(
                "Tenant context set for event {EventType} with EstablishmentId {EstablishmentId}",
                typeof(TNotification).Name,
                message.EstablishmentId
            );
        }
        else
        {
            logger.LogDebug(
                "Event {EventType} does not contain EstablishmentId property. Skipping tenant context setup.",
                typeof(TNotification).Name);
        }

        await next(message, cancellationToken);

        return Unit.Value;
    }
}