using Devlivery.WebApi.Shared.Database.Context;
using Devlivery.WebApi.Shared.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Features.Dashboard;

public static class GetDashboardStats
{
    public record Response(
        int TotalOrders,
        decimal TotalRevenue,
        int PendingOrders,
        int DeliveredOrders,
        decimal AverageOrderValue);

    public static async Task<Ok<ApiResponse<Response>>> Handle(ApplicationDbContext db)
    {
        var orders = await db.Orders.ToListAsync();

        var totalOrders = orders.Count;

        var totalRevenue = orders
            .Where(o => o.Status != "cancelled")
            .Sum(o => o.Total);

        var pendingOrders = orders
            .Count(o => o.Status == "pending" || o.Status == "preparing" || o.Status == "ready");

        var deliveredOrders = orders.Count(o => o.Status == "delivered");

        var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

        var response = new Response(
            totalOrders,
            totalRevenue,
            pendingOrders,
            deliveredOrders,
            averageOrderValue);

        return TypedResults.Ok(ApiResponse<Response>.Ok(response, "Dashboard statistics retrieved successfully"));
    }
}
