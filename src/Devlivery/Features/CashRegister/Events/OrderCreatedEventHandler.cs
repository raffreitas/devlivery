using Devlivery.Features.CashRegister.Infrastructure;
using Devlivery.Features.Orders.Domain.Events;
using Devlivery.Shared.Infrastructure.Persistence;
using Devlivery.Shared.Infrastructure.Tenancy;

using Mediator;

namespace Devlivery.Features.CashRegister.Events;

/// <summary>
/// Handles OrderCreatedEvent to track orders in cash register sessions.
/// This is an example of cross-feature communication using domain events.
/// Tenant context is automatically set by DomainEventTenantBehavior.
/// </summary>
public sealed class OrderCreatedEventHandler(
    ILogger<OrderCreatedEventHandler> logger,
    ICashSessionRepository cashSessionRepository,
    IUnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor
) : INotificationHandler<OrderCreatedEvent>
{
    public async ValueTask Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing OrderCreatedEvent for Order {OrderId} (Total: {Total}, PaymentMethod: {PaymentMethod}, EstablishmentId: {EstablishmentId})",
            notification.OrderId,
            notification.Total,
            notification.PaymentMethod,
            tenantAccessor.Tenant.Id);

        var activeSession = await cashSessionRepository.GetActiveSessionAsync(cancellationToken);
        if (activeSession is null)
        {
            logger.LogDebug(
                "No active cash session found for establishment {EstablishmentId}. Order {OrderId} will not be tracked in cash register.",
                tenantAccessor.Tenant.Id,
                notification.OrderId);
            return;
        }

        activeSession.RecordOrder(notification.Total, notification.PaymentMethod.ToString());
        await cashSessionRepository.UpdateAsync(activeSession, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Order {OrderId} successfully recorded in cash session {SessionId}",
            notification.OrderId,
            activeSession.Id);
    }
}