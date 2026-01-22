using Devlivery.Common.Domain.Enums;
using Devlivery.Features.CashRegister.Infrastructure;
using Devlivery.Features.Orders.Domain;
using Devlivery.Features.Orders.Domain.Enums;
using Devlivery.Features.Orders.Domain.Events;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Infrastructure.Tenancy;
using Devlivery.Shared.Infrastructure.Persistence;

using Mediator;

namespace Devlivery.Features.CashRegister.Events;

public sealed class OrderChangeCalculatedEventHandler(
    ILogger<OrderChangeCalculatedEventHandler> logger,
    ICashSessionRepository cashSessionRepository,
    IUnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor,
    IOrderRepository orderRepository
) : INotificationHandler<OrderChangeCalculatedEvent>
{
    public async ValueTask Handle(OrderChangeCalculatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing OrderChangeCalculatedEvent for Order {OrderId} (Change: {Change}, EstablishmentId: {EstablishmentId})",
            notification.OrderId,
            notification.Change,
            tenantAccessor.Tenant.Id);

        if (notification.Change <= 0)
        {
            logger.LogDebug("No change to record for Order {OrderId}", notification.OrderId);
            return;
        }

        // Ensure the order exists and that there was a cash payment involved
        var order = await orderRepository.GetByIdAsync(notification.OrderId, cancellationToken);
        if (order is null)
        {
            logger.LogWarning("Order {OrderId} not found; cannot record change.", notification.OrderId);
            return;
        }

        var hasCashPayment = order.Payments.Any(p => p.PaymentMethod == PaymentMethod.Cash && p.PaymentStatus != PaymentStatus.Cancelled && p.Amount > 0);
        if (!hasCashPayment)
        {
            logger.LogDebug("Order {OrderId} has no cash payment; skipping change recording.", notification.OrderId);
            return;
        }

        var activeSession = await cashSessionRepository.GetActiveSessionAsync(cancellationToken);
        if (activeSession is null)
        {
            logger.LogWarning(
                "No active cash session found for establishment {EstablishmentId}. Change for Order {OrderId} will not be recorded.",
                tenantAccessor.Tenant.Id,
                notification.OrderId);
            return;
        }

        // Idempotency: do not create duplicate change entries for the same order
        if (activeSession.HasChangeFor(notification.OrderId))
        {
            logger.LogInformation("Change entry for Order {OrderId} already exists in session {SessionId}", notification.OrderId, activeSession.Id);
            return;
        }

        activeSession.AddChange(notification.OrderId, notification.Change, PaymentMethod.Cash);

        await cashSessionRepository.UpdateAsync(activeSession, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Change for Order {OrderId} recorded in cash session {SessionId}. Change: {Change}",
            notification.OrderId,
            activeSession.Id,
            notification.Change);
    }
}
