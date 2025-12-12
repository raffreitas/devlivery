using Devlivery.Features.Orders.Domain.Events;

using Mediator;

namespace Devlivery.Features.CashRegister.Events;

/// <summary>
/// Handles OrderCreatedEvent to track orders in cash register sessions.
/// This is an example of cross-feature communication using domain events.
/// </summary>
public sealed class OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger)
    : INotificationHandler<OrderCreatedEvent>
{
    public ValueTask Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Order {OrderId} created with total {Total} and payment method {PaymentMethod}",
            notification.OrderId,
            notification.Total,
            notification.PaymentMethod);

        // In the future, this could:
        // 1. Update active cash session totals in real-time
        // 2. Track payment breakdown
        // 3. Trigger notifications
        // For now, we just log the event as demonstration

        return ValueTask.CompletedTask;
    }
}