using Devlivery.Domain.SeedWork;

namespace Devlivery.Domain.Aggregates.Orders.Events;

public sealed record OrderChangeCalculatedEvent(Guid OrderId, Guid EstablishmentId, decimal Change, DateTime CalculatedAt) : DomainEventBase;