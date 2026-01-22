using Devlivery.Common.Domain.Enums;
using Devlivery.Common.SeedWork;

namespace Devlivery.Features.Orders.Domain.Events;

public sealed record OrderPaymentAddedEvent(Guid OrderId, Guid PaymentId, Guid EstablishmentId, PaymentMethod PaymentMethod, decimal Amount, DateTime CreatedAt) : DomainEventBase;
