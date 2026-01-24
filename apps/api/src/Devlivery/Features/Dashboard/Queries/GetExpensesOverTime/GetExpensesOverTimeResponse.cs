namespace Devlivery.Features.Dashboard.Queries.GetExpensesOverTime;

public sealed record ExpenseTimeSeriesItem(string Date, decimal Total);

public sealed record GetExpensesOverTimeResponse(List<ExpenseTimeSeriesItem> Data);