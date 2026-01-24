using Devlivery.Shared.SeedWork;
using Devlivery.Shared.Domain.Enums;

namespace Devlivery.Features.Orders.Domain.Events;

public sealed record OrderPaymentAddedEvent(Guid OrderId, Guid PaymentId, Guid EstablishmentId, PaymentMethod PaymentMethod, decimal Amount, DateTime CreatedAt) : DomainEventBase;
