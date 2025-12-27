using Devlivery.Features.Orders.Domain.Enums;

using FluentResults;

using Mediator;

namespace Devlivery.Features.Orders.Queries.GetAllOrders;

public sealed record GetAllOrdersQuery(DateTime? StartDate, DateTime? EndDate, PaymentMethod? PaymentMethod)
    : IQuery<Result<List<GetAllOrdersResponse>>>;