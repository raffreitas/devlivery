using Devlivery.Domain.Aggregates.CashRegister.Abstractions;
using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.Aggregates.Orders.Events;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Infrastructure.Tenancy;

using Mediator;

namespace Devlivery.Features.CashRegister.Events;

public sealed class OrderStatusChangedEventHandler(
    ILogger<OrderStatusChangedEventHandler> logger,
    ICashSessionRepository cashSessionRepository,
    IUnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor
) : INotificationHandler<OrderStatusChangedEvent>
{
    public async ValueTask Handle(OrderStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.NewStatus != OrderStatus.Canceled)
            return;

        logger.LogInformation(
            "Processing OrderStatusChangedEvent (Canceled) for Order {OrderId}, Establishment {EstablishmentId}",
            notification.OrderId,
            tenantAccessor.Tenant.Id);

        var activeSession = await cashSessionRepository.GetActiveSessionAsync(cancellationToken);
        if (activeSession is null)
        {
            logger.LogWarning("No active cash session found for establishment {EstablishmentId}.",
                tenantAccessor.Tenant.Id);
            return;
        }

        var payments = activeSession.Movements.Where(p =>
                p.EntryType == CashSessionEntryType.Payment && p.RelatedOrderId == notification.OrderId && p.OrderPaymentId != null)
            .ToList();

        if (payments.Count == 0)
        {
            logger.LogInformation("No payments in active session correlated to order {OrderId}.", notification.OrderId);
            return;
        }

        foreach (var payment in payments)
        {
            if (payment.OrderPaymentId is null || activeSession.HasReversalFor(payment.OrderPaymentId.Value))
                continue;

            activeSession.AddReversal(
                originalOrderPaymentId: payment.OrderPaymentId.Value,
                amount: Math.Abs(payment.Amount),
                paymentMethod: payment.PaymentMethod!.Value,
                reason: "Pedido Cancelado",
                relatedOrderId: notification.OrderId);
        }

        await cashSessionRepository.UpdateAsync(activeSession, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Recorded reversals for canceled order {OrderId} in session {SessionId}.",
            notification.OrderId, activeSession.Id);
    }
}