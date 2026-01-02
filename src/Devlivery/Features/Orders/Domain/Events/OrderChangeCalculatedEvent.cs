using Devlivery.Shared.SeedWork;

namespace Devlivery.Features.Orders.Domain.Events;

public sealed record OrderChangeCalculatedEvent(Guid OrderId, Guid EstablishmentId, decimal Change, DateTime CalculatedAt) : DomainEventBase;
