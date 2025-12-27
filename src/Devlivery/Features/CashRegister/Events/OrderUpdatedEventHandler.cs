using Devlivery.Features.CashRegister.Infrastructure;
using Devlivery.Features.Orders.Domain.Events;
using Devlivery.Shared.Infrastructure.Persistence;
using Devlivery.Shared.Infrastructure.Tenancy;

using Mediator;

namespace Devlivery.Features.CashRegister.Events;

/// <summary>
/// Handles OrderUpdatedEvent to adjust cash session totals when order total changes.
/// This ensures cash register remains accurate when items are added/removed/changed.
/// Tenant context is automatically set by DomainEventTenantBehavior.
/// </summary>
public sealed class OrderUpdatedEventHandler(
    ILogger<OrderUpdatedEventHandler> logger,
    ICashSessionRepository cashSessionRepository,
    IUnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor
) : INotificationHandler<OrderUpdatedEvent>
{
    public async ValueTask Handle(OrderUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing OrderUpdatedEvent for Order {OrderId} ({OldTotal} -> {NewTotal}, EstablishmentId: {EstablishmentId})",
            notification.OrderId,
            notification.OldTotal,
            notification.NewTotal,
            tenantAccessor.Tenant.Id);

        var activeSession = await cashSessionRepository.GetActiveSessionAsync(cancellationToken);
        if (activeSession is null)
        {
            logger.LogDebug(
                "No active cash session found for establishment {EstablishmentId}. Order {OrderId} update will not affect cash register.",
                tenantAccessor.Tenant.Id,
                notification.OrderId);
            return;
        }

        // Use encapsulated business logic in the aggregate
        activeSession.AdjustOrderTotal(
            notification.OldTotal,
            notification.NewTotal,
            notification.PaymentMethod.ToString()
        );

        await cashSessionRepository.UpdateAsync(activeSession, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Order {OrderId} update processed in cash session {SessionId}. Total difference: {Difference}",
            notification.OrderId,
            activeSession.Id,
            notification.NewTotal - notification.OldTotal);
    }
}