namespace Devlivery.Features.Dashboard.Queries.GetDashboardStats;

public sealed record GetDashboardStatsResponse(
    int TotalOrders,
    decimal TotalRevenue,
    int PendingOrders,
    int DeliveredOrders,
    decimal AverageOrderValue);

