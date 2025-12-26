using FluentResults;
using Mediator;

namespace Devlivery.Features.Dashboard.Queries.GetPaymentBreakdown;

public sealed record GetPaymentBreakdownQuery(DateTime? StartDate, DateTime? EndDate)
    : IQuery<Result<GetPaymentBreakdownResponse>>;

