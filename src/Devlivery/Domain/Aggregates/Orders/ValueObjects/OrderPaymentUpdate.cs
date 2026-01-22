using Devlivery.Domain.Common.Enums;

namespace Devlivery.Domain.Aggregates.Orders;

public sealed record OrderPaymentUpdate(Guid? Id, PaymentMethod Method, decimal Amount);
