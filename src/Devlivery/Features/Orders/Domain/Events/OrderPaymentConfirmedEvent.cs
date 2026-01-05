using Devlivery.Shared.Domain.Enums;
using Devlivery.Shared.SeedWork;

namespace Devlivery.Features.Orders.Domain.Events;

public sealed record OrderPaymentConfirmedEvent(
    Guid OrderId,
    Guid PaymentId,
    Guid EstablishmentId,
    PaymentMethod PaymentMethod,
    decimal Amount,
    decimal OrderTotal
) : DomainEventBase;