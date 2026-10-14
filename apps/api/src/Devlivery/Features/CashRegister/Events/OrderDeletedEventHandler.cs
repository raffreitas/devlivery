using Devlivery.Common.Events;
using Devlivery.Infrastructure.Identity.Authentication;
using Devlivery.Domain.Aggregates.CashRegister.Abstractions;
using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Domain.Aggregates.Orders.Events;
using Devlivery.Infrastructure.Tenancy;

namespace Devlivery.Features.CashRegister.Events;

public sealed class OrderDeletedEventHandler(
    ILogger<OrderDeletedEventHandler> logger,
    ICashSessionRepository cashSessionRepository,
    ICurrentUserAccessor currentUserAccessor,
    ITenantAccessor tenantAccessor
) : IDomainEventHandler<OrderDeletedEvent>
{
    public async Task HandleAsync(OrderDeletedEvent domainEvent, CancellationToken cancellationToken)
    {
        var actor = await currentUserAccessor.ResolveAsync(cancellationToken);
        logger.LogInformation(
            "Processing OrderDeletedEvent for Order {OrderId} (Total: {Total}, EstablishmentId: {EstablishmentId})",
            domainEvent.OrderId,
            domainEvent.Total,
            tenantAccessor.Tenant.Id);

        var activeSession = await cashSessionRepository.GetActiveSessionAsync(cancellationToken);
        if (activeSession is null)
        {
            logger.LogWarning(
                "No active cash session found for establishment {EstablishmentId}. No reversal recorded for order {OrderId}.",
                tenantAccessor.Tenant.Id,
                domainEvent.OrderId);
            return;
        }

        var matches = activeSession.Movements
            .Where(p => p.EntryType == CashSessionEntryType.Payment && p.OrderPaymentId is not null &&
                        p.RelatedOrderId == domainEvent.OrderId)
            .ToList();

        if (matches.Count == 0)
        {
            logger.LogInformation("No matching payments found in active session for order {OrderId}.",
                domainEvent.OrderId);
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
                createdBy: actor.Id,
                relatedOrderId: domainEvent.OrderId);
        }

        logger.LogInformation("Reversal entries recorded for order {OrderId} in session {SessionId}",
            domainEvent.OrderId, activeSession.Id);
    }
}
