using Devlivery.Features.CashRegister.Infrastructure;
using Devlivery.Features.Orders.Domain.Events;
using Devlivery.Features.Orders.Infrastructure;
using Devlivery.Shared.Infrastructure.Persistence;

using Mediator;

namespace Devlivery.Features.CashRegister.Events;

/// <summary>
/// Handles OrderCreatedEvent to track orders in cash register sessions.
/// This is an example of cross-feature communication using domain events.
/// </summary>
public sealed class OrderCreatedEventHandler(
    ILogger<OrderCreatedEventHandler> logger,
    ICashSessionRepository cashSessionRepository,
    IUnitOfWork unitOfWork
) : INotificationHandler<OrderCreatedEvent>
{
    public async ValueTask Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Order {OrderId} created with total {Total} and payment method {PaymentMethod}",
            notification.OrderId,
            notification.Total,
            notification.PaymentMethod);

        var activeSession = await cashSessionRepository.GetActiveSessionAsync(cancellationToken);
        if (activeSession is null)
        {
            logger.LogWarning("No active cash session found for Order {OrderId}", notification.OrderId);
            return;
        }

        activeSession.RecordOrder(notification.Total, notification.PaymentMethod.ToString());
        await cashSessionRepository.UpdateAsync(activeSession, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}