using Devlivery.Shared.SeedWork;

namespace Devlivery.Features.Orders.Domain.Events;

/// <summary>
/// Event raised when a new order is created.
/// This allows other features (like CashRegister) to react to new orders.
/// </summary>
public sealed record OrderCreatedEvent(
    Guid OrderId,
    Guid EstablishmentId,
    string CustomerName,
    decimal Total,
    PaymentMethod PaymentMethod,
    DateTime CreatedAt
) : DomainEventBase;