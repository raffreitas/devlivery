using Devlivery.Features.CashRegister.Infrastructure;
using Devlivery.Features.Orders.Domain.Enums;
using Devlivery.Features.Orders.Domain.Events;
using Devlivery.Shared.Infrastructure.Persistence;
using Devlivery.Shared.Infrastructure.Tenancy;

using Mediator;

namespace Devlivery.Features.CashRegister.Events;

/// <summary>
/// Handles OrderDeletedEvent to remove the order from cash session totals.
/// Only processes non-canceled orders since canceled orders were already removed.
/// Tenant context is automatically set by DomainEventTenantBehavior.
/// </summary>
public sealed class OrderDeletedEventHandler(
    ILogger<OrderDeletedEventHandler> logger,
    ICashSessionRepository cashSessionRepository,
    IUnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor
) : INotificationHandler<OrderDeletedEvent>
{
    public async ValueTask Handle(OrderDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing OrderDeletedEvent for Order {OrderId} (Status: {Status}, Total: {Total}, EstablishmentId: {EstablishmentId})",
            notification.OrderId,
            notification.Status,
            notification.Total,
            tenantAccessor.Tenant.Id);

        // If order was already canceled, it was already removed from cash session
        if (notification.Status == OrderStatus.Canceled)
        {
            logger.LogDebug(
                "Order {OrderId} was already canceled. No cash session adjustment needed for deletion.",
                notification.OrderId);
            return;
        }

        var activeSession = await cashSessionRepository.GetActiveSessionAsync(cancellationToken);
        if (activeSession is null)
        {
            logger.LogDebug(
                "No active cash session found for establishment {EstablishmentId}. Deleted order {OrderId} will not affect cash register.",
                tenantAccessor.Tenant.Id,
                notification.OrderId);
            return;
        }

        // Remove the order from the session
        activeSession.RemoveOrder(notification.Total, notification.PaymentMethod.ToString());
        await cashSessionRepository.UpdateAsync(activeSession, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Order {OrderId} deletion processed in cash session {SessionId}. Removed amount: {Amount}",
            notification.OrderId,
            activeSession.Id,
            notification.Total);
    }
}