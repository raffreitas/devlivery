using Devlivery.Features.Dashboard.Queries.GetDashboardStats;
using Devlivery.Features.Dashboard.Queries.GetExpensesByCategory;
using Devlivery.Features.Dashboard.Queries.GetExpensesByStatus;
using Devlivery.Features.Dashboard.Queries.GetExpensesOverTime;
using Devlivery.Features.Dashboard.Queries.GetExpenseSummary;
using Devlivery.Features.Dashboard.Queries.GetOrdersByStatus;
using Devlivery.Features.Dashboard.Queries.GetPaymentBreakdown;
using Devlivery.Features.Dashboard.Queries.GetSalesOverTime;
using Devlivery.Features.Dashboard.Queries.GetTopProducts;
using Devlivery.Features.Dashboard.Queries.GetUpcomingExpenses;

namespace Devlivery.Features.Dashboard;

public static class DashboardFeature
{
    public static IServiceCollection AddDashboardFeature(this IServiceCollection services)
    {
        // Handlers are automatically discovered by Mediator
        // No manual registration needed when using IQueryHandler/ICommandHandler

        return services;
    }

    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard").WithTags("Dashboard");

        GetDashboardStatsEndpoint.MapEndpoint(group);
        GetPaymentBreakdownEndpoint.MapEndpoint(group);
        GetOrdersByStatusEndpoint.MapEndpoint(group);
        GetSalesOverTimeEndpoint.MapEndpoint(group);
        GetTopProductsEndpoint.MapEndpoint(group);
        GetExpensesByCategoryEndpoint.MapEndpoint(group);
        GetExpensesByStatusEndpoint.MapEndpoint(group);
        GetExpensesOverTimeEndpoint.MapEndpoint(group);
        GetExpenseSummaryEndpoint.MapEndpoint(group);
        GetUpcomingExpensesEndpoint.MapEndpoint(group);

        return app;
    }
}