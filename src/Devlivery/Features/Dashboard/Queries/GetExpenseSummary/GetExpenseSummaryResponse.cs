namespace Devlivery.Features.Dashboard.Queries.GetExpenseSummary;

public sealed record GetExpenseSummaryResponse(
    decimal Total,
    decimal Paid,
    decimal Pending,
    decimal Overdue,
    int Count);

