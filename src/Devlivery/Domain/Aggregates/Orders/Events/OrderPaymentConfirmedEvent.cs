using Devlivery.Domain.Common.Enums;
using Devlivery.Domain.SeedWork;

namespace Devlivery.Features.Orders.Domain.Events;

public sealed record OrderPaymentConfirmedEvent(
    Guid OrderId,
    Guid PaymentId,
    Guid EstablishmentId,
    PaymentMethod PaymentMethod,
    decimal Amount,
    decimal OrderTotal
) : DomainEventBase;