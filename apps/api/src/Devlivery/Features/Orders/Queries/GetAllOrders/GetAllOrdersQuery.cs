using Devlivery.Domain.Common.Enums;

namespace Devlivery.Features.Orders.Queries.GetAllOrders;

public sealed record GetAllOrdersQuery(DateOnly? StartDate, DateOnly? EndDate, PaymentMethod? PaymentMethod);