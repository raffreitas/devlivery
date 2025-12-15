using Devlivery.Shared.SeedWork;

namespace Devlivery.Features.Orders.Domain.Events;

/// <summary>
/// Event raised when an order status changes.
/// This is useful for tracking order lifecycle and updating related features.
/// </summary>
public sealed record OrderStatusChangedEvent(
    Guid OrderId,
    Guid EstablishmentId,
    OrderStatus OldStatus,
    OrderStatus NewStatus,
    PaymentMethod PaymentMethod,
    decimal TotalAmount,
    DateTime ChangedAt
) : DomainEventBase;