using Devlivery.Common.Events;
using Devlivery.Infrastructure.Identity.Authentication;
using Devlivery.Domain.Aggregates.CashRegister.Abstractions;
using Devlivery.Domain.Aggregates.Orders.Events;
using Devlivery.Domain.SeedWork;
using Devlivery.Infrastructure.Tenancy;

namespace Devlivery.Features.CashRegister.Events;

/// <summary>
/// Handles OrderPaymentConfirmedEvent to add payments to the CashSession ledger.
/// This implements the Ledger Pattern - payments are only recorded when confirmed (order delivered).
/// </summary>
public sealed class OrderPaymentConfirmedEventHandler(
    ILogger<OrderPaymentConfirmedEventHandler> logger,
    ICashSessionRepository cashSessionRepository,
    ICurrentUserAccessor currentUserAccessor,
    ITenantAccessor tenantAccessor
) : IDomainEventHandler<OrderPaymentConfirmedEvent>
{
    public async Task HandleAsync(OrderPaymentConfirmedEvent domainEvent, CancellationToken cancellationToken)
    {
        var actor = await currentUserAccessor.ResolveAsync(cancellationToken);
        logger.LogInformation(
            "Processing OrderPaymentConfirmedEvent for Order {OrderId}, Payment {PaymentId} (Amount: {Amount}, Method: {PaymentMethod}, EstablishmentId: {EstablishmentId})",
            domainEvent.OrderId,
            domainEvent.PaymentId,
            domainEvent.Amount,
            domainEvent.PaymentMethod,
            tenantAccessor.Tenant.Id);

        var activeSession = await cashSessionRepository.GetActiveSessionAsync(cancellationToken);
        if (activeSession is null)
        {
            logger.LogWarning(
                "No active cash session found for establishment {EstablishmentId}. Payment {PaymentId} will not be recorded in cash register.",
                tenantAccessor.Tenant.Id,
                domainEvent.PaymentId);
            throw new DomainException("Abra o caixa antes de confirmar o pagamento do pedido.");
        }

        activeSession.AddPayment(
            orderPaymentId: domainEvent.PaymentId,
            amount: domainEvent.Amount,
            paymentMethod: domainEvent.PaymentMethod,
            createdBy: actor.Id,
            relatedOrderId: domainEvent.OrderId
        );

        logger.LogInformation(
            "Payment {PaymentId} successfully recorded in cash session {SessionId} ledger. Amount: {Amount}, Method: {PaymentMethod}",
            domainEvent.PaymentId,
            activeSession.Id,
            domainEvent.Amount,
            domainEvent.PaymentMethod);
    }
}
