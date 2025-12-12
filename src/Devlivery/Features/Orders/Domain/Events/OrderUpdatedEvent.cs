using Devlivery.Shared.SeedWork;

namespace Devlivery.Features.Orders.Domain.Events;

/// <summary>
/// Event raised when an order is updated (items, customer details, etc).
/// </summary>
public sealed record OrderUpdatedEvent(
    Guid OrderId,
    Guid EstablishmentId,
    decimal NewTotal,
    DateTime UpdatedAt) : DomainEventBase;
