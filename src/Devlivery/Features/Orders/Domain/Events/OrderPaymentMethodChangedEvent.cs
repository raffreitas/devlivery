using Devlivery.Shared.SeedWork;

namespace Devlivery.Features.Orders.Domain.Events;

public sealed record OrderPaymentMethodChangedEvent(
    Guid OrderId,
    Guid EstablishmentId,
    PaymentMethod OldPaymentMethod,
    PaymentMethod NewPaymentMethod,
    decimal TotalAmount,
    DateTime ChangedAt
) : DomainEventBase;