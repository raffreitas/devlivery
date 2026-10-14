namespace Devlivery.Features.Dashboard.Queries.GetPaymentBreakdown;

public sealed record GetPaymentBreakdownQuery(DateOnly? StartDate, DateOnly? EndDate);