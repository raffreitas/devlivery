using Devlivery.Domain.Common.Enums;

namespace Devlivery.Features.Orders.Queries.GetAllOrders;

public sealed record GetAllOrdersQuery(DateTime? StartDate, DateTime? EndDate, PaymentMethod? PaymentMethod);