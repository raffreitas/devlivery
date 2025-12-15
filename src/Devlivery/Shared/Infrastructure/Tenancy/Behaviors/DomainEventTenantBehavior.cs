using Mediator;

namespace Devlivery.Shared.Infrastructure.Tenancy.Behaviors;

/// <summary>
/// Pipeline behavior that automatically sets the tenant context for domain events.
/// This eliminates the need for manual tenant validation in each event handler.
/// </summary>
/// <remarks>
/// Any domain event that has an EstablishmentId property will have its tenant
/// automatically registered in the TenantAccessor before the handler executes.
/// </remarks>
public sealed class DomainEventTenantBehavior<TNotification>(
    ITenantAccessor tenantAccessor,
    ILogger<DomainEventTenantBehavior<TNotification>> logger)
    : IPipelineBehavior<TNotification, ValueTask>
    where TNotification : INotification
{
    public async ValueTask Handle(
        TNotification notification,
        MessageHandlerDelegate<TNotification, ValueTask> next,
        CancellationToken cancellationToken)
    {
        // Try to extract EstablishmentId from the event using reflection
        var establishmentIdProperty = typeof(TNotification).GetProperty("EstablishmentId");
        
        if (establishmentIdProperty is not null && establishmentIdProperty.PropertyType == typeof(Guid))
        {
            var establishmentId = (Guid)establishmentIdProperty.GetValue(notification)!;
            
            // Register tenant context for this event processing
            var tenant = new Tenant(establishmentId, string.Empty); // Name is not needed for event processing
            tenantAccessor.Register(tenant);
            
            logger.LogDebug(
                "Tenant context set for event {EventType} with EstablishmentId {EstablishmentId}",
                typeof(TNotification).Name,
                establishmentId);
        }
        else
        {
            logger.LogDebug(
                "Event {EventType} does not contain EstablishmentId property. Skipping tenant context setup.",
                typeof(TNotification).Name);
        }

        // Continue to the handler
        await next(notification, cancellationToken);
    }
}
