using Devlivery.Common.Domain.Enums;

namespace Devlivery.Features.Orders.Domain;

public sealed record OrderPaymentUpdate(Guid? Id, PaymentMethod Method, decimal Amount);
