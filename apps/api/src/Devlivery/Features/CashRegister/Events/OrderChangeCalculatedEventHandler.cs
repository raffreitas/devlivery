using Devlivery.Common.Events;
using Devlivery.Infrastructure.Identity.Authentication;
using Devlivery.Domain.Aggregates.CashRegister.Abstractions;
using Devlivery.Domain.Aggregates.Orders.Abstractions;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.Aggregates.Orders.Events;
using Devlivery.Domain.Common.Enums;
using Devlivery.Infrastructure.Tenancy;

namespace Devlivery.Features.CashRegister.Events;

public sealed class OrderChangeCalculatedEventHandler(
    ILogger<OrderChangeCalculatedEventHandler> logger,
    ICashSessionRepository cashSessionRepository,
    ICurrentUserAccessor currentUserAccessor,
    ITenantAccessor tenantAccessor,
    IOrderRepository orderRepository
) : IDomainEventHandler<OrderChangeCalculatedEvent>
{
    public async Task HandleAsync(OrderChangeCalculatedEvent domainEvent, CancellationToken cancellationToken)
    {
        var actor = await currentUserAccessor.ResolveAsync(cancellationToken);
        logger.LogInformation(
            "Processing OrderChangeCalculatedEvent for Order {OrderId} (Change: {Change}, EstablishmentId: {EstablishmentId})",
            domainEvent.OrderId,
            domainEvent.Change,
            tenantAccessor.Tenant.Id);

        if (domainEvent.Change <= 0)
        {
            logger.LogDebug("No change to record for Order {OrderId}", domainEvent.OrderId);
            return;
        }

        var order = await orderRepository.GetByIdAsync(domainEvent.OrderId, cancellationToken);
        if (order is null)
        {
            logger.LogWarning("Order {OrderId} not found; cannot record change.", domainEvent.OrderId);
            return;
        }

        var hasCashPayment = order.Payments.Any(p =>
            p.PaymentMethod == PaymentMethod.Cash && p.PaymentStatus != PaymentStatus.Cancelled && p.Amount > 0);
        if (!hasCashPayment)
        {
            logger.LogDebug("Order {OrderId} has no cash payment; skipping change recording.", domainEvent.OrderId);
            return;
        }

        var activeSession = await cashSessionRepository.GetActiveSessionAsync(cancellationToken);
        if (activeSession is null)
        {
            logger.LogInformation(
                "Order {OrderId} delivered outside a cash session for establishment {EstablishmentId}. Change remains recorded on the order.",
                domainEvent.OrderId,
                tenantAccessor.Tenant.Id);
            return;
        }

        // Idempotency: do not create duplicate change entries for the same order
        if (activeSession.HasChangeFor(domainEvent.OrderId))
        {
            logger.LogInformation("Change entry for Order {OrderId} already exists in session {SessionId}",
                domainEvent.OrderId, activeSession.Id);
            return;
        }

        activeSession.AddChange(domainEvent.OrderId, domainEvent.Change, actor.Id, PaymentMethod.Cash);

        logger.LogInformation(
            "Change for Order {OrderId} recorded in cash session {SessionId}. Change: {Change}",
            domainEvent.OrderId,
            activeSession.Id,
            domainEvent.Change);
    }
}
