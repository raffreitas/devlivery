using Devlivery.Features.CashRegister.Infrastructure;
using Devlivery.Features.Orders.Domain;
using Devlivery.Features.Orders.Domain.Events;
using Devlivery.Shared.Infrastructure.Persistence;
using Devlivery.Shared.Infrastructure.Tenancy;

using Mediator;

namespace Devlivery.Features.CashRegister.Events;

/// <summary>
/// Handles OrderStatusChangedEvent to track order lifecycle in cash register.
/// Currently processes order cancellations to adjust cash session totals.
/// Tenant context is automatically set by DomainEventTenantBehavior.
/// </summary>
public sealed class OrderStatusChangedEventHandler(
    ILogger<OrderStatusChangedEventHandler> logger,
    ICashSessionRepository cashSessionRepository,
    IUnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor
) : INotificationHandler<OrderStatusChangedEvent>
{
    public async ValueTask Handle(OrderStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing OrderStatusChangedEvent for Order {OrderId} ({OldStatus} -> {NewStatus}, EstablishmentId: {EstablishmentId})",
            notification.OrderId,
            notification.OldStatus,
            notification.NewStatus,
            tenantAccessor.Tenant.Id);

        // Only process cancellations
        if (notification.NewStatus != OrderStatus.Canceled)
        {
            logger.LogDebug(
                "Order {OrderId} status changed to {NewStatus}. No cash register action required.",
                notification.OrderId,
                notification.NewStatus);
            return;
        }

        var activeSession = await cashSessionRepository.GetActiveSessionAsync(cancellationToken);
        if (activeSession is null)
        {
            logger.LogDebug(
                "No active cash session found for establishment {EstablishmentId}. Canceled order {OrderId} will not affect cash register.",
                tenantAccessor.Tenant.Id,
                notification.OrderId);
            return;
        }

        activeSession.RemoveOrder(notification.TotalAmount, notification.PaymentMethod.ToString());
        await cashSessionRepository.UpdateAsync(activeSession, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Order {OrderId} cancellation processed in cash session {SessionId}. Reversed amount: {Amount}",
            notification.OrderId,
            activeSession.Id,
            notification.TotalAmount);
    }
}