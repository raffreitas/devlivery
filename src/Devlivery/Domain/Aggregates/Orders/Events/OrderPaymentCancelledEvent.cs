using Devlivery.Domain.SeedWork;

namespace Devlivery.Domain.Aggregates.Orders.Events;

public sealed record OrderPaymentCancelledEvent(Guid OrderId, Guid PaymentId, Guid EstablishmentId, DateTime CancelledAt) : DomainEventBase;
