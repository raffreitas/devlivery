using System.Net;
using System.Net.Http.Json;

using Devlivery.Domain.Aggregates.CashRegister;
using Devlivery.Domain.Aggregates.Orders.Entities;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.Common.Enums;
using Devlivery.Features.CashRegister.Commands.CloseCashSession;
using Devlivery.Features.CashRegister.Queries.GetCashMovements;
using Devlivery.Features.CashRegister.Queries.GetCashOverview;
using Devlivery.Infrastructure.Http.Models;
using Devlivery.Tests.Common.Api;
using Devlivery.Tests.Common.Builders;

using Microsoft.EntityFrameworkCore;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Api;

[Collection(ApiCollection.Name)]
[Trait("Category", "Integration Tests")]
public sealed class CashRegisterV2ApiTests(ApiFactory api)
{
    [Fact]
    public async Task GivenMixedPaymentsChangeRefundAndDeposit_WhenReadingV2_ThenReconcilesSummaryAndPaginatedLedger()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync("Ana Lima");
        var session = new CashSession(tenant.EstablishmentId, tenant.Operator.Id, tenant.Operator.Name, 100m, "Turno");
        var orderId = Guid.NewGuid();
        var refundedOrder = Guid.NewGuid();
        var refundedPayment = Guid.NewGuid();
        session.AddPayment(Guid.NewGuid(), 100m, PaymentMethod.Cash, orderId, tenant.Operator.Id);
        session.AddPayment(Guid.NewGuid(), 50m, PaymentMethod.Pix, orderId, tenant.Operator.Id);
        session.AddChange(orderId, 20m, tenant.Operator.Id);
        session.AddDeposit(50m, tenant.Operator.Id, "Troco");
        session.AddPayment(refundedPayment, 40m, PaymentMethod.Cash, refundedOrder, tenant.Operator.Id);
        session.AddReversal(refundedPayment, 40m, PaymentMethod.Cash, "Estorno", refundedOrder, tenant.Operator.Id);
        await tenant.SeedAsync(session);
        var sameTime = session.StartAt.AddSeconds(1);
        await tenant.WithDbContextAsync(db => db.CashSessionMovements.Where(m => m.CashSessionId == session.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.CreatedAt, sameTime)));

        // Act
        var overview = await ApiTenant.ReadDataAsync<GetCashOverviewResponse>(
            await tenant.Client.GetAsync("/api/v2/cash-register/overview"));
        var pages = new List<GetCashMovementsResponse>();
        for (var page = 1; page <= 4; page++)
        {
            var response = await tenant.Client.GetAsync($"/api/v2/cash-register/sessions/{session.Id}/movements?page={page}&pageSize=2");
            response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
            var resource = await response.Content.ReadFromJsonAsync<ApiResource<GetCashMovementsResponse[]>>(ApiTenant.JsonOptions);
            var metadata = resource.ShouldNotBeNull().Metadata.ShouldNotBeNull();
            metadata.TotalCount.ShouldBe(7);
            metadata.TotalPages.ShouldBe(4);
            metadata.CurrentPage.ShouldBe(page);
            pages.AddRange(resource.Data);
        }
        var repeat = await ApiTenant.ReadDataAsync<GetCashMovementsResponse[]>(
            await tenant.Client.GetAsync($"/api/v2/cash-register/sessions/{session.Id}/movements?page=1&pageSize=2"));

        // Assert
        var summary = overview.ActiveSession.ShouldNotBeNull();
        summary.TotalRevenue.ShouldBe(130m);
        summary.TotalOrders.ShouldBe(1);
        summary.NetCashReceived.ShouldBe(80m);
        summary.ExpectedCashAmount.ShouldBe(230m);
        pages.Select(m => m.Id).Distinct().Count().ShouldBe(7);
        pages.Where(m => m.Method == PaymentMethod.Cash).Sum(m => m.SignedAmount).ShouldBe(summary.ExpectedCashAmount);
        pages.Where(m => m.Type is "Payment" or "Refund" or "Change").Sum(m => m.SignedAmount).ShouldBe(summary.TotalRevenue);
        pages.ShouldContain(m => m.Type == "Change" && m.SignedAmount == -20m);
        pages.ShouldContain(m => m.Type == "Refund" && m.SignedAmount == -40m);
        pages.ShouldAllBe(m => m.AuthorName == "Ana Lima");
        pages.Last().Type.ShouldBe("Opening");
        repeat.Select(m => m.Id).ShouldBe(pages.Take(2).Select(m => m.Id));
    }

    [Fact]
    public async Task GivenOldMixedAndConfirmedOrders_WhenReadingReceivables_ThenPredictsOnlyOutstandingNetCash()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var product = new ProductBuilder().WithEstablishmentId(tenant.EstablishmentId).WithPrice(100m).Build();
        var confirmed = new OrderPayment(tenant.EstablishmentId, PaymentMethod.Cash, 30m);
        confirmed.Confirm();
        var cancelled = new OrderPayment(tenant.EstablishmentId, PaymentMethod.Cash, 100m);
        cancelled.Cancel();
        var cancelledElectronic = new OrderPayment(tenant.EstablishmentId, PaymentMethod.Pix, 60m);
        cancelledElectronic.Cancel();
        var oldOrder = new OrderBuilder().WithEstablishmentId(tenant.EstablishmentId).WithDeliveryFee(0m)
            .WithItems(new OrderItemBuilder().WithProduct(product).WithEstablishmentId(tenant.EstablishmentId).WithQuantity(1).Build())
            .WithPaymentMethod(PaymentMethod.Cash, 150m).WithPaymentMethod(PaymentMethod.Pix, 20m).WithPayment(cancelledElectronic).Build();
        var partial = new OrderBuilder().WithEstablishmentId(tenant.EstablishmentId).WithDeliveryFee(0m)
            .WithItems(new OrderItemBuilder().WithProduct(product).WithEstablishmentId(tenant.EstablishmentId).WithQuantity(1).Build())
            .WithPayment(confirmed).WithPaymentMethod(PaymentMethod.Cash, 50m).WithPaymentMethod(PaymentMethod.Pix, 20m).Build();
        var onlyElectronic = new OrderBuilder().WithEstablishmentId(tenant.EstablishmentId).WithDeliveryFee(0m)
            .WithItems(new OrderItemBuilder().WithProduct(product).WithEstablishmentId(tenant.EstablishmentId).WithQuantity(1).Build())
            .WithPaymentMethod(PaymentMethod.Pix, 100m).Build();
        var cancelledOrder = new OrderBuilder().WithEstablishmentId(tenant.EstablishmentId).WithDeliveryFee(0m)
            .WithItems(new OrderItemBuilder().WithProduct(product).WithEstablishmentId(tenant.EstablishmentId).WithQuantity(1).Build())
            .WithPaymentMethod(PaymentMethod.Cash, 100m).WithStatus(OrderStatus.Canceled).Build();
        var cancelledParcel = new OrderBuilder().WithEstablishmentId(tenant.EstablishmentId).WithDeliveryFee(0m)
            .WithItems(new OrderItemBuilder().WithProduct(product).WithEstablishmentId(tenant.EstablishmentId).WithQuantity(1).Build())
            .WithPayment(cancelled).Build();
        var delivered = new OrderBuilder().WithEstablishmentId(tenant.EstablishmentId).WithDeliveryFee(0m)
            .WithItems(new OrderItemBuilder().WithProduct(product).WithEstablishmentId(tenant.EstablishmentId).WithQuantity(1).Build())
            .WithPaymentMethod(PaymentMethod.Cash, 100m).WithStatus(OrderStatus.Delivered).Build();
        var session = new CashSessionBuilder().WithEstablishmentId(tenant.EstablishmentId).WithAttendantId(tenant.Operator.Id).WithOpeningAmount(100m).Build();
        await tenant.SeedAsync(product, oldOrder, partial, onlyElectronic, cancelledOrder, cancelledParcel, delivered, session);
        var previousDay = session.StartAt.AddDays(-1);
        await tenant.WithDbContextAsync(db => db.Orders.Where(o => o.Id == oldOrder.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.CreatedAt, previousDay)));
        var other = await api.CreateTenantAsync();
        var otherProduct = new ProductBuilder().WithEstablishmentId(other.EstablishmentId).WithPrice(900m).Build();
        var otherOrder = new OrderBuilder().WithEstablishmentId(other.EstablishmentId).WithDeliveryFee(0m)
            .WithItems(new OrderItemBuilder().WithProduct(otherProduct).WithEstablishmentId(other.EstablishmentId).WithQuantity(1).Build())
            .WithPaymentMethod(PaymentMethod.Cash, 900m).Build();
        await other.SeedAsync(otherProduct, otherOrder);

        // Act
        var overview = await ApiTenant.ReadDataAsync<GetCashOverviewResponse>(
            await tenant.Client.GetAsync("/api/v2/cash-register/overview"));

        // Assert
        overview.ReceivableCash.ShouldNotBeNull().Amount.ShouldBe(130m);
        overview.ReceivableCash.OrderCount.ShouldBe(2);
        overview.ActiveSession.ShouldNotBeNull().ExpectedCashAmount.ShouldBe(session.OpeningAmount);
    }

    [Fact]
    public async Task GivenOtherTenantSession_WhenReadingV2_ThenIsolatesOverviewAndReturns404ForLedger()
    {
        // Arrange
        var owner = await api.CreateTenantAsync();
        var intruder = await api.CreateTenantAsync();
        var session = new CashSessionBuilder().WithEstablishmentId(owner.EstablishmentId).WithAttendantId(owner.Operator.Id).Build();
        await owner.SeedAsync(session);
        using var anonymous = api.CreateClient();

        // Act
        var overview = await ApiTenant.ReadDataAsync<GetCashOverviewResponse>(
            await intruder.Client.GetAsync("/api/v2/cash-register/overview"));
        var crossTenant = await intruder.Client.GetAsync($"/api/v2/cash-register/sessions/{session.Id}/movements");
        var missing = await owner.Client.GetAsync($"/api/v2/cash-register/sessions/{Guid.NewGuid()}/movements");
        var unauthenticated = await anonymous.GetAsync("/api/v2/cash-register/overview");
        var unauthenticatedLedger = await anonymous.GetAsync($"/api/v2/cash-register/sessions/{session.Id}/movements");

        // Assert
        overview.ActiveSession.ShouldBeNull();
        overview.LastClosedSession.ShouldBeNull();
        overview.ReceivableCash.ShouldBeNull();
        crossTenant.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        missing.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        unauthenticated.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        unauthenticatedLedger.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(0, 50)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task GivenInvalidPagination_WhenReadingLedger_ThenReturns422(int page, int pageSize)
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();

        // Act
        var response = await tenant.Client.GetAsync($"/api/v2/cash-register/sessions/{Guid.NewGuid()}/movements?page={page}&pageSize={pageSize}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task GivenOpenSession_WhenClosingV1WithDifferenceAndNoNote_ThenV2ShowsLastClosedWithoutChangingV1()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var session = new CashSessionBuilder().WithEstablishmentId(tenant.EstablishmentId).WithAttendantId(tenant.Operator.Id).WithOpeningAmount(100m).Build();
        await tenant.SeedAsync(session);

        // Act
        var closed = await tenant.Client.PatchAsJsonAsync($"/api/v1/cash-register/sessions/{session.Id}/close",
            new CloseCashSessionCommand(session.Id, 90m, null), ApiTenant.JsonOptions);
        var overview = await ApiTenant.ReadDataAsync<GetCashOverviewResponse>(
            await tenant.Client.GetAsync("/api/v2/cash-register/overview"));

        // Assert
        closed.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        overview.ActiveSession.ShouldBeNull();
        overview.ReceivableCash.ShouldBeNull();
        overview.LastClosedSession.ShouldNotBeNull().Id.ShouldBe(session.Id);
        overview.LastClosedSession.Difference.ShouldBe(-10m);
    }
}
