using Devlivery.Domain.SeedWork;
using Devlivery.Features.Orders.Domain.Enums;

namespace Devlivery.Features.Orders.Domain.Events;

/// <summary>
/// Event raised when an order is deleted.
/// This allows other features (like CashRegister) to adjust their totals.
/// </summary>
public sealed record OrderDeletedEvent(
    Guid OrderId,
    Guid EstablishmentId,
    decimal Total,
    OrderStatus Status,
    DateTime DeletedAt
) : DomainEventBase;