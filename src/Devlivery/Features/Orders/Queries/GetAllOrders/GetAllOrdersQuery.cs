using Devlivery.Features.Orders.Domain;

namespace Devlivery.Features.Orders.Queries.GetAllOrders;

public sealed record GetAllOrdersQuery(DateTime? StartDate, DateTime? EndDate, PaymentMethod? PaymentMethod);