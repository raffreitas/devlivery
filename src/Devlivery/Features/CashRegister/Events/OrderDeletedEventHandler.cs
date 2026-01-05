using Devlivery.Features.CashRegister.Domain.Enums;
using Devlivery.Features.CashRegister.Infrastructure;
using Devlivery.Features.Orders.Domain.Events;
using Devlivery.Shared.Infrastructure.Persistence;
using Devlivery.Shared.Infrastructure.Tenancy;

using Mediator;

namespace Devlivery.Features.CashRegister.Events;

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
            "Processing OrderDeletedEvent for Order {OrderId} (Total: {Total}, EstablishmentId: {EstablishmentId})",
            notification.OrderId,
            notification.Total,
            tenantAccessor.Tenant.Id);

        var activeSession = await cashSessionRepository.GetActiveSessionAsync(cancellationToken);
        if (activeSession is null)
        {
            logger.LogWarning(
                "No active cash session found for establishment {EstablishmentId}. No reversal recorded for order {OrderId}.",
                tenantAccessor.Tenant.Id,
                notification.OrderId);
            return;
        }

        // Try to find payments correlated to the deleted order
        var matches = activeSession.Movements
            .Where(p => p.EntryType == CashSessionEntryType.Payment && p.OrderPaymentId is not null &&
                        (p.RelatedOrderId == notification.OrderId || p.OrderPaymentId == notification.OrderId))
            .ToList();

        if (matches.Count == 0)
        {
            logger.LogInformation("No matching payments found in active session for order {OrderId}.",
                notification.OrderId);
            return;
        }

        foreach (var payment in matches)
        {
            if (payment.OrderPaymentId is null || activeSession.HasReversalFor(payment.OrderPaymentId.Value))
                continue;

            activeSession.AddReversal(
                originalOrderPaymentId: payment.OrderPaymentId.Value,
                amount: Math.Abs(payment.Amount),
                paymentMethod: payment.PaymentMethod!.Value,
                reason: "Pedido Excluído",
                relatedOrderId: notification.OrderId);
        }

        await cashSessionRepository.UpdateAsync(activeSession, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Reversal entries recorded for order {OrderId} in session {SessionId}",
            notification.OrderId, activeSession.Id);
    }
}