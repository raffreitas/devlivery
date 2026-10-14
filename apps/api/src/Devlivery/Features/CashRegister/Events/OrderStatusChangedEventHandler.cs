using Devlivery.Common.Events;
using Devlivery.Infrastructure.Identity.Authentication;
using Devlivery.Domain.Aggregates.CashRegister.Abstractions;
using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.Aggregates.Orders.Events;
using Devlivery.Infrastructure.Tenancy;

namespace Devlivery.Features.CashRegister.Events;

public sealed class OrderStatusChangedEventHandler(
    ILogger<OrderStatusChangedEventHandler> logger,
    ICashSessionRepository cashSessionRepository,
    ICurrentUserAccessor currentUserAccessor,
    ITenantAccessor tenantAccessor
) : IDomainEventHandler<OrderStatusChangedEvent>
{
    public async Task HandleAsync(OrderStatusChangedEvent domainEvent, CancellationToken cancellationToken)
    {
        var actor = await currentUserAccessor.ResolveAsync(cancellationToken);
        if (domainEvent.NewStatus != OrderStatus.Canceled)
            return;

        logger.LogInformation(
            "Processing OrderStatusChangedEvent (Canceled) for Order {OrderId}, Establishment {EstablishmentId}",
            domainEvent.OrderId,
            tenantAccessor.Tenant.Id);

        var activeSession = await cashSessionRepository.GetActiveSessionAsync(cancellationToken);
        if (activeSession is null)
        {
            logger.LogWarning("No active cash session found for establishment {EstablishmentId}.",
                tenantAccessor.Tenant.Id);
            return;
        }

        var payments = activeSession.Movements.Where(p =>
                p.EntryType == CashSessionEntryType.Payment && p.RelatedOrderId == domainEvent.OrderId &&
                p.OrderPaymentId != null)
            .ToList();

        if (payments.Count == 0)
        {
            logger.LogInformation("No payments in active session correlated to order {OrderId}.", domainEvent.OrderId);
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
                createdBy: actor.Id,
                relatedOrderId: domainEvent.OrderId);
        }

        await cashSessionRepository.UpdateAsync(activeSession, cancellationToken);

        logger.LogInformation("Recorded reversals for canceled order {OrderId} in session {SessionId}.",
            domainEvent.OrderId, activeSession.Id);
    }
}
