using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.SeedWork;

namespace Devlivery.Domain.Aggregates.Orders.Events;

public sealed record OrderStatusChangedEvent(
    Guid OrderId,
    Guid EstablishmentId,
    OrderStatus OldStatus,
    OrderStatus NewStatus,
    decimal TotalAmount,
    DateTime ChangedAt
) : DomainEventBase;