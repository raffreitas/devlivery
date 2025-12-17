using Devlivery.Features.CashRegister.Infrastructure;
using Devlivery.Features.Orders.Domain.Events;
using Devlivery.Shared.Infrastructure.Persistence;
using Devlivery.Shared.Infrastructure.Tenancy;

using Mediator;

namespace Devlivery.Features.CashRegister.Events;

public sealed class OrderPaymentMethodChangedEventHandler(
    ILogger<OrderPaymentMethodChangedEventHandler> logger,
    ICashSessionRepository cashSessionRepository,
    IUnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor
) : INotificationHandler<OrderPaymentMethodChangedEvent>
{
    public async ValueTask Handle(OrderPaymentMethodChangedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing OrderPaymentMethodChangedEvent for Order {OrderId} ({OldPaymentMethod} -> {NewPaymentMethod}, EstablishmentId: {EstablishmentId})",
            notification.OrderId,
            notification.OldPaymentMethod,
            notification.NewPaymentMethod,
            tenantAccessor.Tenant.Id);

        var activeSession = await cashSessionRepository.GetActiveSessionAsync(cancellationToken);
        if (activeSession is null)
        {
            logger.LogDebug(
                "No active cash session found for establishment {EstablishmentId}. Change Payment Method for order {OrderId} will not affect cash register.",
                tenantAccessor.Tenant.Id,
                notification.OrderId);
            return;
        }

        activeSession.RemoveOrder(notification.TotalAmount, notification.OldPaymentMethod.ToString());
        activeSession.RecordOrder(notification.TotalAmount, notification.NewPaymentMethod.ToString());

        await cashSessionRepository.UpdateAsync(activeSession, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}