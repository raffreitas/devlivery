using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.SeedWork;

namespace Devlivery.Domain.Aggregates.Orders.Events;

/// <summary>
/// Event raised when an order status changes.
/// This is useful for tracking order lifecycle and updating related features.
/// </summary>
public sealed record OrderStatusChangedEvent(
    Guid OrderId,
    Guid EstablishmentId,
    OrderStatus OldStatus,
    OrderStatus NewStatus,
    decimal TotalAmount,
    DateTime ChangedAt
) : DomainEventBase;