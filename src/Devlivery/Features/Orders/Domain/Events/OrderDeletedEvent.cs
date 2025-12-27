using Devlivery.Features.Orders.Domain.Enums;
using Devlivery.Shared.SeedWork;

namespace Devlivery.Features.Orders.Domain.Events;

/// <summary>
/// Event raised when an order is deleted.
/// This allows other features (like CashRegister) to adjust their totals.
/// </summary>
public sealed record OrderDeletedEvent(
    Guid OrderId,
    Guid EstablishmentId,
    decimal Total,
    PaymentMethod PaymentMethod,
    OrderStatus Status,
    DateTime DeletedAt
) : DomainEventBase;