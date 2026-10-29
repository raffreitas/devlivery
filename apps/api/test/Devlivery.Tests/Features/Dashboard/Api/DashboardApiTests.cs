using System.Net;

using Devlivery.Common.Time;
using Devlivery.Domain.Aggregates.Expenses;
using Devlivery.Domain.Aggregates.Orders;
using Devlivery.Domain.Aggregates.Orders.Entities;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.Aggregates.Products;
using Devlivery.Domain.Common.Enums;
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
using Devlivery.Features.Expenses.Queries.GetAllExpenses;
using Devlivery.Tests.Common.Api;
using Devlivery.Tests.Common.Builders;

using Shouldly;

namespace Devlivery.Tests.Features.Dashboard.Api;

[Collection(ApiCollection.Name)]
[Trait("Category", "Integration Tests")]
public sealed class DashboardApiTests(ApiFactory api)
{
    // Expense dashboards classify by "today" in São Paulo, so their scenarios are relative to the current day.
    private static readonly DateOnly Today = TimeProvider.System.GetBrazilToday();

    [Fact]
    public async Task GivenPaidExpensesWithDifferentDueDates_WhenFilteringDashboard_ThenPaidTotalsMatchPaymentSeries()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var category = await SeedCategoryAsync(tenant, "Operacional");
        var paymentDay = new DateOnly(2026, 10, 20);
        await SeedExpenseAsync(tenant, category, 100m, e => e.WithDueDate(paymentDay.AddMonths(1)).AsPaid(paymentDay));
        await SeedExpenseAsync(tenant, category, 50m, e => e.WithDueDate(paymentDay).AsPaid(paymentDay.AddMonths(-1)));
        await SeedExpenseAsync(tenant, category, 20m, e => e.WithDueDate(paymentDay));
        await SeedExpenseAsync(tenant, category, 999m, e => e.WithDueDate(paymentDay).AsCancelled());
        const string period = "?startDate=2026-10-20&endDate=2026-10-20";

        // Act
        var summary = await ApiTenant.ReadDataAsync<GetExpenseSummaryResponse>(
            await tenant.Client.GetAsync($"/api/v1/dashboard/expense-summary{period}"));
        var series = await ApiTenant.ReadDataAsync<GetExpensesOverTimeResponse>(
            await tenant.Client.GetAsync($"/api/v1/dashboard/expenses-over-time{period}"));
        var statuses = await ApiTenant.ReadDataAsync<GetExpensesByStatusResponse>(
            await tenant.Client.GetAsync($"/api/v1/dashboard/expenses-by-status{period}"));
        var categories = await ApiTenant.ReadDataAsync<GetExpensesByCategoryResponse>(
            await tenant.Client.GetAsync($"/api/v1/dashboard/expenses-by-category{period}"));

        // Assert
        summary.Paid.ShouldBe(100m);
        summary.Paid.ShouldBe(series.Data.Sum(p => p.Total));
        summary.Total.ShouldBe(120m);
        summary.Count.ShouldBe(2);
        statuses.Statuses.Single(s => s.Status == ExpenseDisplayStatus.Paid).Total.ShouldBe(summary.Paid);
        statuses.Statuses.ShouldNotContain(s => s.Status == ExpenseDisplayStatus.Cancelled);
        categories.Categories.ShouldHaveSingleItem().Total.ShouldBe(summary.Total);
    }

    [Fact]
    public async Task GivenOrdersInEveryStatus_WhenGettingStats_ThenExcludesCanceledFromRevenueAndAverage()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var pizza = await SeedProductAsync(tenant, "Pizza", 10m);
        await SeedOrderAsync(tenant, [(pizza, 4)], OrderStatus.Pending);
        await SeedOrderAsync(tenant, [(pizza, 3)], OrderStatus.Preparing);
        await SeedOrderAsync(tenant, [(pizza, 5)], OrderStatus.Delivered);
        await SeedOrderAsync(tenant, [(pizza, 10)], OrderStatus.Canceled);

        // Act
        var response = await tenant.Client.GetAsync("/api/v1/dashboard/stats");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var stats = await ApiTenant.ReadDataAsync<GetDashboardStatsResponse>(response);
        stats.ShouldBe(new GetDashboardStatsResponse(TotalOrders: 3, TotalRevenue: 120m, PendingOrders: 2,
            DeliveredOrders: 1, AverageOrderValue: 40m));
    }

    [Fact]
    public async Task GivenOrdersInEveryStatus_WhenGettingOrdersByStatus_ThenCountsEachStatus()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var pizza = await SeedProductAsync(tenant, "Pizza", 10m);
        await SeedOrderAsync(tenant, [(pizza, 1)], OrderStatus.Pending);
        await SeedOrderAsync(tenant, [(pizza, 1)], OrderStatus.Pending);
        await SeedOrderAsync(tenant, [(pizza, 1)], OrderStatus.Ready);
        await SeedOrderAsync(tenant, [(pizza, 1)], OrderStatus.Delivered);
        await SeedOrderAsync(tenant, [(pizza, 1)], OrderStatus.Canceled);

        // Act
        var response = await tenant.Client.GetAsync("/api/v1/dashboard/orders-by-status");

        // Assert
        (await ApiTenant.ReadDataAsync<GetOrdersByStatusResponse>(response))
            .ShouldBe(new GetOrdersByStatusResponse(Pending: 2, Preparing: 0, Ready: 1, Delivered: 1, Canceled: 1));
    }

    [Fact]
    public async Task GivenDeliveredAndOpenOrders_WhenGettingPaymentBreakdown_ThenSumsConfirmedPaymentsNetOfChange()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var pizza = await SeedProductAsync(tenant, "Pizza", 70m);
        var soda = await SeedProductAsync(tenant, "Refrigerante", 40m);
        await SeedOrderAsync(tenant, [(pizza, 1)], OrderStatus.Delivered, Payment(tenant, PaymentMethod.Cash, 100m));
        await SeedOrderAsync(tenant, [(soda, 1)], OrderStatus.Delivered, Payment(tenant, PaymentMethod.Pix, 40m));
        await SeedOrderAsync(tenant, [(soda, 1)], OrderStatus.Pending, Payment(tenant, PaymentMethod.CreditCard, 40m));

        // Act
        var response = await tenant.Client.GetAsync("/api/v1/dashboard/payment-breakdown");

        // Assert
        var breakdown = await ApiTenant.ReadDataAsync<GetPaymentBreakdownResponse>(response);
        breakdown.Breakdown.ShouldBe(new Dictionary<PaymentMethod, decimal>
        {
            [PaymentMethod.Cash] = 70m,
            [PaymentMethod.Pix] = 40m,
            [PaymentMethod.CreditCard] = 0m,
            [PaymentMethod.DebitCard] = 0m
        }, ignoreOrder: true);
        breakdown.Total.ShouldBe(110m);
    }

    [Fact]
    public async Task GivenOrdersAcrossDays_WhenGettingSalesOverTime_ThenGroupsBySaoPauloDayExcludingCanceled()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var pizza = await SeedProductAsync(tenant, "Pizza", 10m);
        await SeedOrderAsync(tenant, [(pizza, 1)], OrderStatus.Delivered, createdAt: Utc(2026, 10, 14, 2, 0));
        await SeedOrderAsync(tenant, [(pizza, 2)], OrderStatus.Delivered, createdAt: Utc(2026, 10, 14, 15, 0));
        await SeedOrderAsync(tenant, [(pizza, 3)], OrderStatus.Pending, createdAt: Utc(2026, 10, 14, 20, 0));
        await SeedOrderAsync(tenant, [(pizza, 9)], OrderStatus.Canceled, createdAt: Utc(2026, 10, 14, 21, 0));
        await SeedOrderAsync(tenant, [(pizza, 4)], OrderStatus.Delivered, createdAt: Utc(2026, 10, 15, 15, 0));

        // Act
        var response = await tenant.Client.GetAsync("/api/v1/dashboard/sales-over-time");

        // Assert
        var sales = await ApiTenant.ReadDataAsync<GetSalesOverTimeResponse>(response);
        sales.Data.ShouldBe([
            new SalesTimeSeriesItem("2026-10-13", 10m),
            new SalesTimeSeriesItem("2026-10-14", 50m),
            new SalesTimeSeriesItem("2026-10-15", 40m)
        ]);
    }

    [Fact]
    public async Task GivenDateRange_WhenGettingStats_ThenConsidersOnlyOrdersCreatedWithinIt()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var pizza = await SeedProductAsync(tenant, "Pizza", 10m);
        await SeedOrderAsync(tenant, [(pizza, 1)], OrderStatus.Delivered, createdAt: Utc(2026, 10, 13, 15, 0));
        await SeedOrderAsync(tenant, [(pizza, 2)], OrderStatus.Delivered, createdAt: Utc(2026, 10, 14, 15, 0));

        // Act
        var response = await tenant.Client.GetAsync("/api/v1/dashboard/stats?startDate=2026-10-14&endDate=2026-10-14");

        // Assert
        var stats = await ApiTenant.ReadDataAsync<GetDashboardStatsResponse>(response);
        stats.TotalOrders.ShouldBe(1);
        stats.TotalRevenue.ShouldBe(20m);
    }

    [Fact]
    public async Task GivenOrderedProducts_WhenGettingTopProducts_ThenRanksByQuantityExcludingCanceledOrders()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var pizza = await SeedProductAsync(tenant, "Pizza", 40m);
        var soda = await SeedProductAsync(tenant, "Refrigerante", 5m);
        var dessert = await SeedProductAsync(tenant, "Pudim", 12m);
        await SeedOrderAsync(tenant, [(pizza, 1), (soda, 2)], OrderStatus.Delivered);
        await SeedOrderAsync(tenant, [(pizza, 1), (soda, 2)], OrderStatus.Pending);
        await SeedOrderAsync(tenant, [(dessert, 10)], OrderStatus.Canceled);

        // Act
        var response = await tenant.Client.GetAsync("/api/v1/dashboard/top-products");

        // Assert
        var top = await ApiTenant.ReadDataAsync<GetTopProductsResponse>(response);
        top.Products.ShouldBe([new TopProductItem("Refrigerante", 4), new TopProductItem("Pizza", 2)]);
    }

    [Fact]
    public async Task GivenExpensesInEveryStatus_WhenGettingSummary_ThenSplitsPaidPendingAndOverdue()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var category = await SeedCategoryAsync(tenant, "Operacional");
        await SeedExpenseAsync(tenant, category, 100m, e => e.WithDueDate(Today.AddDays(-5)).AsPaid(Today.AddDays(-5)));
        await SeedExpenseAsync(tenant, category, 50m, e => e.WithDueDate(Today.AddDays(5)));
        await SeedExpenseAsync(tenant, category, 20m, e => e.WithDueDate(Today));
        await SeedExpenseAsync(tenant, category, 30m, e => e.WithDueDate(Today.AddDays(-1)));
        await SeedExpenseAsync(tenant, category, 10m, e => e.WithDueDate(Today.AddDays(-1)).AsCancelled());

        // Act
        var response = await tenant.Client.GetAsync("/api/v1/dashboard/expense-summary");

        // Assert
        var summary = await ApiTenant.ReadDataAsync<GetExpenseSummaryResponse>(response);
        summary.Paid.ShouldBe(100m);
        summary.Pending.ShouldBe(70m); // due today still counts as pending
        summary.Overdue.ShouldBe(30m);
    }

    [Fact]
    public async Task GivenExpensesInSubcategories_WhenGettingByCategory_ThenRollsThemUpToParentWithPercentages()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var operational = await SeedCategoryAsync(tenant, "Operacional");
        var supplies = await SeedCategoryAsync(tenant, "Insumos", operational);
        var staff = await SeedCategoryAsync(tenant, "Pessoal");
        await SeedExpenseAsync(tenant, supplies, 60m);
        await SeedExpenseAsync(tenant, operational, 40m);
        await SeedExpenseAsync(tenant, staff, 300m);

        // Act
        var response = await tenant.Client.GetAsync("/api/v1/dashboard/expenses-by-category");

        // Assert
        var byCategory = await ApiTenant.ReadDataAsync<GetExpensesByCategoryResponse>(response);
        byCategory.Categories.ShouldBe([
            new ExpenseCategoryItem("Pessoal", 300m, 75m),
            new ExpenseCategoryItem("Operacional", 100m, 25m)
        ]);
    }

    [Fact]
    public async Task GivenExpensesInEveryStatus_WhenGettingByStatus_ThenGroupsByDisplayStatus()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var category = await SeedCategoryAsync(tenant, "Operacional");
        await SeedExpenseAsync(tenant, category, 100m, e => e.WithDueDate(Today.AddDays(-5)).AsPaid(Today));
        await SeedExpenseAsync(tenant, category, 50m, e => e.WithDueDate(Today.AddDays(5)));
        await SeedExpenseAsync(tenant, category, 20m, e => e.WithDueDate(Today));
        await SeedExpenseAsync(tenant, category, 30m, e => e.WithDueDate(Today.AddDays(-1)));
        await SeedExpenseAsync(tenant, category, 15m, e => e.WithDueDate(Today.AddDays(-2)));

        // Act
        var response = await tenant.Client.GetAsync("/api/v1/dashboard/expenses-by-status");

        // Assert
        var byStatus = await ApiTenant.ReadDataAsync<GetExpensesByStatusResponse>(response);
        byStatus.Statuses.ShouldBe([
            new ExpenseStatusItem(ExpenseDisplayStatus.DueToday, 1, 20m),
            new ExpenseStatusItem(ExpenseDisplayStatus.Overdue, 2, 45m),
            new ExpenseStatusItem(ExpenseDisplayStatus.Paid, 1, 100m),
            new ExpenseStatusItem(ExpenseDisplayStatus.Pending, 1, 50m)
        ]);
    }

    [Fact]
    public async Task GivenPaidExpenses_WhenGettingExpensesOverTime_ThenGroupsByPaymentDateWithinRange()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var category = await SeedCategoryAsync(tenant, "Operacional");
        var day = new DateOnly(2026, 5, 10);
        await SeedExpenseAsync(tenant, category, 100m, e => e.AsPaid(day));
        await SeedExpenseAsync(tenant, category, 50m, e => e.AsPaid(day));
        await SeedExpenseAsync(tenant, category, 30m, e => e.AsPaid(day.AddDays(1)));
        await SeedExpenseAsync(tenant, category, 999m, e => e.AsPaid(day.AddDays(30)));
        await SeedExpenseAsync(tenant, category, 70m, e => e.WithDueDate(day));

        // Act
        var response = await tenant.Client.GetAsync("/api/v1/dashboard/expenses-over-time?startDate=2026-05-01&endDate=2026-05-31");

        // Assert
        var overTime = await ApiTenant.ReadDataAsync<GetExpensesOverTimeResponse>(response);
        overTime.Data.ShouldBe([
            new ExpenseTimeSeriesItem("2026-05-10", 150m),
            new ExpenseTimeSeriesItem("2026-05-11", 30m)
        ]);
    }

    [Fact]
    public async Task GivenExpensesAroundTheWindow_WhenGettingUpcoming_ThenReturnsUnpaidDueWithinDaysSortedByDueDate()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var operational = await SeedCategoryAsync(tenant, "Operacional");
        var supplies = await SeedCategoryAsync(tenant, "Insumos", operational);
        await SeedExpenseAsync(tenant, supplies, 30m, e => e.WithDueDate(Today.AddDays(3)).WithDescription("Em 3 dias"));
        await SeedExpenseAsync(tenant, supplies, 20m, e => e.WithDueDate(Today).WithDescription("Hoje"));
        await SeedExpenseAsync(tenant, supplies, 10m, e => e.WithDueDate(Today.AddDays(7)).WithDescription("Em 7 dias"));
        await SeedExpenseAsync(tenant, supplies, 40m, e => e.WithDueDate(Today.AddDays(8)).WithDescription("Fora da janela"));
        await SeedExpenseAsync(tenant, supplies, 50m, e => e.WithDueDate(Today.AddDays(-1)).WithDescription("Vencida"));
        await SeedExpenseAsync(tenant, supplies, 60m, e => e.WithDueDate(Today.AddDays(2)).AsPaid(Today).WithDescription("Paga"));

        // Act
        var response = await tenant.Client.GetAsync("/api/v1/dashboard/upcoming-expenses?days=7");

        // Assert
        var upcoming = await ApiTenant.ReadDataAsync<GetUpcomingExpensesResponse>(response);
        upcoming.Expenses.Select(e => (e.Description, e.Status)).ShouldBe([
            ("Hoje", ExpenseDisplayStatus.DueToday),
            ("Em 3 dias", ExpenseDisplayStatus.Pending),
            ("Em 7 dias", ExpenseDisplayStatus.Pending)
        ]);
        var category = upcoming.Expenses[0].Category;
        category.Name.ShouldBe("Operacional");
        category.Subcategories.ShouldHaveSingleItem().Name.ShouldBe("Insumos");
    }

    public static TheoryData<string> TenantScopedReadRoutes => new()
    {
        "/api/v1/dashboard/stats",
        "/api/v1/dashboard/orders-by-status",
        "/api/v1/dashboard/payment-breakdown",
        "/api/v1/dashboard/sales-over-time",
        "/api/v1/dashboard/top-products",
        "/api/v1/dashboard/expense-summary",
        "/api/v1/dashboard/expenses-by-category",
        "/api/v1/dashboard/expenses-by-status",
        "/api/v1/dashboard/expenses-over-time",
        "/api/v1/dashboard/upcoming-expenses?days=7",
        "/api/v1/expenses"
    };

    [Theory]
    [MemberData(nameof(TenantScopedReadRoutes))]
    public async Task GivenDataInAnotherTenant_WhenReadingRoute_ThenRespondsAsAnEmptyTenant(string route)
    {
        // Arrange
        var owner = await api.CreateTenantAsync();
        var pizza = await SeedProductAsync(owner, "Pizza do outro tenant", 43.21m);
        await SeedOrderAsync(owner, [(pizza, 2)], OrderStatus.Delivered, Payment(owner, PaymentMethod.Cash, 100m));
        var category = await SeedCategoryAsync(owner, "Categoria do outro tenant");
        await SeedExpenseAsync(owner, category, 12.34m, e => e.WithDueDate(Today).AsPaid(Today));
        await SeedExpenseAsync(owner, category, 56.78m, e => e.WithDueDate(Today.AddDays(1)));
        var intruder = await api.CreateTenantAsync();
        var empty = await api.CreateTenantAsync();

        // Act
        var ownerResponse = await owner.Client.GetAsync(route);
        var intruderResponse = await intruder.Client.GetAsync(route);
        var emptyResponse = await empty.Client.GetAsync(route);

        // Assert
        new[] { ownerResponse, intruderResponse, emptyResponse }
            .ShouldAllBe(response => response.StatusCode == HttpStatusCode.OK);
        var ownerBody = await ownerResponse.Content.ReadAsStringAsync();
        var intruderBody = await intruderResponse.Content.ReadAsStringAsync();
        var emptyBody = await emptyResponse.Content.ReadAsStringAsync();
        ownerBody.ShouldNotBe(emptyBody);
        intruderBody.ShouldBe(emptyBody);
    }

    private static DateTime Utc(int year, int month, int day, int hour, int minute) =>
        new(year, month, day, hour, minute, 0, DateTimeKind.Utc);

    private static OrderPayment Payment(ApiTenant tenant, PaymentMethod method, decimal amount) =>
        new OrderPaymentBuilder().WithEstablishmentId(tenant.EstablishmentId)
            .WithPaymentMethod(method).WithAmount(amount).Build();

    private static async Task<Product> SeedProductAsync(ApiTenant tenant, string name, decimal price)
    {
        var product = new ProductBuilder().WithName(name).WithPrice(price)
            .WithEstablishmentId(tenant.EstablishmentId).Build();
        await tenant.SeedAsync(product);
        return product;
    }

    private static async Task SeedOrderAsync(ApiTenant tenant, (Product Product, int Quantity)[] items,
        OrderStatus status, OrderPayment? payment = null, DateTime? createdAt = null)
    {
        var builder = new OrderBuilder()
            .WithEstablishmentId(tenant.EstablishmentId)
            .WithDeliveryFee(0m)
            .WithItems(items.Select(i => new OrderItemBuilder().WithProduct(i.Product)
                .WithQuantity(i.Quantity).WithEstablishmentId(tenant.EstablishmentId).Build()).ToArray());
        if (payment is not null)
            builder.WithPayment(payment);
        else
            builder.WithPaymentMethod(PaymentMethod.Pix);
        if (status != OrderStatus.Pending)
            builder.WithStatus(status);

        var order = builder.Build();
        await tenant.SeedAsync(order);

        if (createdAt is not null)
            await tenant.ExecuteSqlAsync($"update orders set created_at = {createdAt.Value} where id = {order.Id}");
    }

    private static async Task<Category> SeedCategoryAsync(ApiTenant tenant, string name, Category? parent = null)
    {
        var builder = new CategoryBuilder().WithEstablishmentId(tenant.EstablishmentId).WithName(name);
        var category = (parent is null ? builder : builder.WithParent(parent)).Build();
        await tenant.SeedAsync(category);
        return category;
    }

    private static async Task SeedExpenseAsync(ApiTenant tenant, Category category, decimal amount,
        Func<ExpenseBuilder, ExpenseBuilder>? configure = null)
    {
        var builder = new ExpenseBuilder().WithEstablishmentId(tenant.EstablishmentId)
            .WithCategoryId(category.Id).WithAmount(amount);
        await tenant.SeedAsync((configure?.Invoke(builder) ?? builder).Build());
    }
}
