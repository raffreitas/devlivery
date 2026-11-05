using System.Net;
using System.Net.Http.Json;
using Devlivery.Domain.Aggregates.CashRegister;
using Devlivery.Domain.Common.Enums;
using Devlivery.Features.CashRegister.Queries;
using Devlivery.Features.CashRegister.Queries.GetCashHistory;
using Devlivery.Infrastructure.Http.Models;
using Devlivery.Tests.Common.Api;
using Devlivery.Tests.Common.Builders;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Api;

[Collection(ApiCollection.Name)]
[Trait("Category", "Integration Tests")]
public sealed class CashHistoryApiTests(ApiFactory api)
{
    private const string Period = "start=2026-11-01&end=2026-11-01";

    [Fact]
    public async Task GivenMixedClosedAndOpenSessions_WhenReadingHistory_ThenReconcilesPeriodTotalsAndStablePages()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync("Ana Lima");
        var order = Guid.NewGuid();
        var refundedOrder = Guid.NewGuid();
        var payment = Guid.NewGuid();
        var a = new CashSessionBuilder().WithEstablishmentId(tenant.EstablishmentId).WithAttendantId(tenant.Operator.Id)
            .WithAttendantName("Ana Lima").WithOpeningAmount(100).WithNotes("Troco inicial")
            .WithPayment(100, PaymentMethod.Cash, order).WithPayment(50, PaymentMethod.Pix, order)
            .WithPayment(40, PaymentMethod.Cash, refundedOrder, payment)
            .WithReversal(40, PaymentMethod.Cash, payment, refundedOrder).WithDeposit(50).Build();
        a.AddChange(order, 20, tenant.Operator.Id);
        a.Close(220, null);
        var b = NewSession(tenant); b.AddPayment(Guid.NewGuid(), 20, PaymentMethod.Pix, order, tenant.Operator.Id); b.Close(110, null);
        var c = NewSession(tenant); c.Close(100, null);
        var open = NewSession(tenant); open.AddPayment(Guid.NewGuid(), 999, PaymentMethod.Cash, Guid.NewGuid(), tenant.Operator.Id);
        var before = NewSession(tenant); before.Close(100, null);
        var after = NewSession(tenant); after.Close(100, null);
        await tenant.SeedAsync(a, b, c, open, before, after);
        var first = new DateTime(2026, 11, 1, 3, 0, 0, DateTimeKind.Utc);
        await SetStart(tenant, a, first); await SetStart(tenant, b, first);
        await SetStart(tenant, c, first.AddDays(1).AddTicks(-10));
        await SetStart(tenant, open, first.AddHours(1));
        await SetStart(tenant, before, first.AddTicks(-10)); await SetStart(tenant, after, first.AddDays(1));

        // Act
        var pages = new List<CashHistorySession>();
        CashHistoryTotals? totals = null;
        for (var page = 1; page <= 4; page++)
        {
            var response = await tenant.Client.GetAsync($"/api/v2/cash-register/sessions?{Period}&page={page}&pageSize=1");
            response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
            var resource = (await response.Content.ReadFromJsonAsync<ApiResource<GetCashHistoryResponse>>(ApiTenant.JsonOptions)).ShouldNotBeNull();
            resource.Metadata.ShouldNotBeNull().TotalCount.ShouldBe(4);
            resource.Metadata.CurrentPage.ShouldBe(page);
            resource.Data.Summary.ShouldBe(totals ?? resource.Data.Summary);
            totals = resource.Data.Summary;
            pages.AddRange(resource.Data.Sessions);
        }
        var detail = await ApiTenant.ReadDataAsync<CashSessionSummary>(await tenant.Client.GetAsync($"/api/v2/cash-register/sessions/{a.Id}"));
        var emptyPageResponse = await tenant.Client.GetAsync($"/api/v2/cash-register/sessions?{Period}&page=5&pageSize=1");
        var emptyPage = (await emptyPageResponse.Content.ReadFromJsonAsync<ApiResource<GetCashHistoryResponse>>(ApiTenant.JsonOptions)).ShouldNotBeNull();
        var legacy = await tenant.Client.GetAsync($"/api/v1/cash-register/sessions?{Period}");

        // Assert
        totals.ShouldBe(new CashHistoryTotals(3, 1, 150, 1, 0, 2));
        emptyPage.Data.Sessions.ShouldBeEmpty();
        emptyPage.Data.Summary.ShouldBe(totals);
        emptyPage.Metadata.ShouldNotBeNull().TotalCount.ShouldBe(4);
        pages.Select(s => s.Id).Distinct().Count().ShouldBe(4);
        pages.Select(s => s.Id).ShouldBe(new[] { c.Id, open.Id }.Concat(new[] { a.Id, b.Id }.OrderDescending()));
        var row = pages.Single(s => s.Id == a.Id);
        row.TotalRevenue.ShouldBe(a.TotalRevenue);
        row.TotalOrders.ShouldBe(a.TotalOrders);
        row.Difference.ShouldBe(-10);
        detail.ExpectedCashAmount.ShouldBe(230);
        detail.TotalRevenue.ShouldBe(row.TotalRevenue);
        detail.Status.ShouldBe("Closed");
        detail.Notes.ShouldBe("Troco inicial");
        pages.Single(s => s.Id == open.Id).Difference.ShouldBeNull();
        legacy.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await legacy.Content.ReadFromJsonAsync<ApiResource<System.Text.Json.JsonElement>>(ApiTenant.JsonOptions)).ShouldNotBeNull().Data.ValueKind
            .ShouldBe(System.Text.Json.JsonValueKind.Array);
    }

    [Fact]
    public async Task GivenAnotherTenantAndAnonymousClient_WhenReadingHistory_ThenDoesNotExposeSessions()
    {
        // Arrange
        var owner = await api.CreateTenantAsync();
        var other = await api.CreateTenantAsync();
        var session = NewSession(owner); session.Close(100, null);
        await owner.SeedAsync(session);
        await SetStart(owner, session, new DateTime(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc));
        using var anonymous = api.CreateClient();

        // Act
        var empty = await ApiTenant.ReadDataAsync<GetCashHistoryResponse>(await other.Client.GetAsync($"/api/v2/cash-register/sessions?{Period}"));
        var denied = await other.Client.GetAsync($"/api/v2/cash-register/sessions/{session.Id}");
        var missing = await owner.Client.GetAsync($"/api/v2/cash-register/sessions/{Guid.NewGuid()}");
        var listUnauthorized = await anonymous.GetAsync($"/api/v2/cash-register/sessions?{Period}");
        var detailUnauthorized = await anonymous.GetAsync($"/api/v2/cash-register/sessions/{session.Id}");

        // Assert
        empty.Sessions.ShouldBeEmpty();
        empty.Summary.ShouldBe(new CashHistoryTotals(0, 0, 0, 0, 0, 0));
        denied.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        missing.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        listUnauthorized.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        detailUnauthorized.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("")]
    [InlineData("start=2026-11-01")]
    [InlineData("start=invalid&end=2026-11-01")]
    public async Task GivenMissingOrMalformedDates_WhenReadingHistory_ThenReturnsBadRequest(string query)
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        // Act
        var response = await tenant.Client.GetAsync($"/api/v2/cash-register/sessions?{query}");
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("start=2026-11-02&end=2026-11-01", HttpStatusCode.UnprocessableEntity)]
    [InlineData(Period + "&page=0", HttpStatusCode.BadRequest)]
    [InlineData(Period + "&pageSize=101", HttpStatusCode.BadRequest)]
    [InlineData(Period + "&pageSize=0", HttpStatusCode.BadRequest)]
    [InlineData(Period + "&page=2147483647", HttpStatusCode.BadRequest)]
    [InlineData(Period + "&page=abc", HttpStatusCode.BadRequest)]
    [InlineData(Period + "&pageSize=1.5", HttpStatusCode.BadRequest)]
    public async Task GivenInvalidHistoryQuery_WhenReading_ThenReturnsExpectedStatus(string query, HttpStatusCode expected)
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        // Act
        var response = await tenant.Client.GetAsync($"/api/v2/cash-register/sessions?{query}");
        // Assert
        response.StatusCode.ShouldBe(expected);
    }

    private static CashSession NewSession(ApiTenant tenant) => new CashSessionBuilder()
        .WithEstablishmentId(tenant.EstablishmentId).WithAttendantId(tenant.Operator.Id).WithOpeningAmount(100).Build();
    private static Task<int> SetStart(ApiTenant tenant, CashSession session, DateTime time) => tenant.WithDbContextAsync(db =>
        db.CashSessions.Where(s => s.Id == session.Id).ExecuteUpdateAsync(u => u.SetProperty(s => s.StartAt, time)));
}
