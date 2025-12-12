using Devlivery.Features.Orders.Domain.Events;
using Mediator;

namespace Devlivery.Features.CashRegister.Events;

/// <summary>
/// Handles OrderStatusChangedEvent to track order lifecycle in cash register.
/// This allows CashRegister to react when orders are completed, canceled, etc.
/// </summary>
public sealed class OrderStatusChangedEventHandler(ILogger<OrderStatusChangedEventHandler> logger)
    : INotificationHandler<OrderStatusChangedEvent>
{
    public ValueTask Handle(OrderStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Order {OrderId} status changed from {OldStatus} to {NewStatus}",
            notification.OrderId,
            notification.OldStatus,
            notification.NewStatus);

        // In the future, this could:
        // 1. Recalculate cash session totals when order is canceled
        // 2. Update payment breakdown when status changes
        // 3. Trigger refund processes
        // For now, we just log the event as demonstration

        return ValueTask.CompletedTask;
    }
}