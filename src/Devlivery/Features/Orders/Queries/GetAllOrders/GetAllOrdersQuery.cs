using Devlivery.Features.Orders.Domain.Enums;

namespace Devlivery.Features.Orders.Queries.GetAllOrders;

public sealed record GetAllOrdersQuery(DateTime? StartDate, DateTime? EndDate, PaymentMethod? PaymentMethod);