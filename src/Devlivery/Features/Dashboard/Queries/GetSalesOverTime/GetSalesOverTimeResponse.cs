namespace Devlivery.Features.Dashboard.Queries.GetSalesOverTime;

public sealed record SalesTimeSeriesItem(string Date, decimal Total);

public sealed record GetSalesOverTimeResponse(List<SalesTimeSeriesItem> Data);

