using Devlivery.Features.CashRegister.Infrastructure;
using Devlivery.Features.Orders.Domain.Events;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Infrastructure.Tenancy;

using Mediator;

namespace Devlivery.Features.CashRegister.Events;

/// <summary>
/// Handles OrderPaymentConfirmedEvent to add payments to the CashSession ledger.
/// This implements the Ledger Pattern - payments are only recorded when confirmed (order delivered).
/// Tenant context is automatically set by DomainEventTenantBehavior.
/// </summary>
public sealed class OrderPaymentConfirmedEventHandler(
    ILogger<OrderPaymentConfirmedEventHandler> logger,
    ICashSessionRepository cashSessionRepository,
    IUnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor
) : INotificationHandler<OrderPaymentConfirmedEvent>
{
    public async ValueTask Handle(OrderPaymentConfirmedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing OrderPaymentConfirmedEvent for Order {OrderId}, Payment {PaymentId} (Amount: {Amount}, Method: {PaymentMethod}, EstablishmentId: {EstablishmentId})",
            notification.OrderId,
            notification.PaymentId,
            notification.Amount,
            notification.PaymentMethod,
            tenantAccessor.Tenant.Id);

        var activeSession = await cashSessionRepository.GetActiveSessionAsync(cancellationToken);
        if (activeSession is null)
        {
            logger.LogWarning(
                "No active cash session found for establishment {EstablishmentId}. Payment {PaymentId} will not be recorded in cash register.",
                tenantAccessor.Tenant.Id,
                notification.PaymentId);
            return;
        }

        // Add payment to the ledger (immutable, idempotent)
        activeSession.AddPayment(
            orderPaymentId: notification.PaymentId,
            amount: notification.Amount,
            paymentMethod: notification.PaymentMethod,
            relatedOrderId: notification.OrderId
        );

        await cashSessionRepository.UpdateAsync(activeSession, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Payment {PaymentId} successfully recorded in cash session {SessionId} ledger. Amount: {Amount}, Method: {PaymentMethod}",
            notification.PaymentId,
            activeSession.Id,
            notification.Amount,
            notification.PaymentMethod);
    }
}