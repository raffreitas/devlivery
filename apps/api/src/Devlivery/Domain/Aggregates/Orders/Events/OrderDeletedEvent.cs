using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.SeedWork;

namespace Devlivery.Domain.Aggregates.Orders.Events;

public sealed record OrderDeletedEvent(
    Guid OrderId,
    Guid EstablishmentId,
    decimal Total,
    OrderStatus Status,
    DateTime DeletedAt
) : DomainEventBase;