using Devlivery.Features.CashRegister.Infrastructure;
using Devlivery.Features.Orders.Domain;
using Devlivery.Features.Orders.Domain.Events;
using Devlivery.Shared.Infrastructure.Persistence;

using Mediator;

namespace Devlivery.Features.CashRegister.Events;

/// <summary>
/// Handles OrderStatusChangedEvent to track order lifecycle in cash register.
/// This allows CashRegister to react when orders are completed, canceled, etc.
/// </summary>
public sealed class OrderStatusChangedEventHandler(
    ILogger<OrderStatusChangedEventHandler> logger,
    ICashSessionRepository cashSessionRepository,
    IUnitOfWork unitOfWork
) : INotificationHandler<OrderStatusChangedEvent>
{
    public async ValueTask Handle(OrderStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Order {OrderId} status changed from {OldStatus} to {NewStatus}",
            notification.OrderId,
            notification.OldStatus,
            notification.NewStatus
        );

        if (notification.NewStatus != OrderStatus.Canceled)
        {
            return;
        }

        var activeSession = await cashSessionRepository.GetActiveSessionAsync(cancellationToken);
        if (activeSession is null)
        {
            logger.LogWarning("No active cash session found for Order {OrderId}", notification.OrderId);
            return;
        }

        activeSession.RecordOrder(notification.TotalAmount * -1, notification.PaymentMethod.ToString());
        await cashSessionRepository.UpdateAsync(activeSession, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}