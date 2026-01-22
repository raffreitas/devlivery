using Devlivery.Domain.SeedWork;

namespace Devlivery.Features.Orders.Domain.Events;

public sealed record OrderPaymentCancelledEvent(Guid OrderId, Guid PaymentId, Guid EstablishmentId, DateTime CancelledAt) : DomainEventBase;
