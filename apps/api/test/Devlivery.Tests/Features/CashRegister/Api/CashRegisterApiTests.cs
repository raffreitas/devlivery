using System.Net;
using System.Net.Http.Json;

using Devlivery.Domain.Aggregates.CashRegister;
using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Domain.Common.Enums;
using Devlivery.Features.CashRegister.Commands.CreateCashDeposit;
using Devlivery.Features.CashRegister.Commands.CreateCashSession;
using Devlivery.Features.CashRegister.Queries.GetActiveCashSession;
using Devlivery.Features.CashRegister.Queries.GetCashSessionById;
using Devlivery.Features.CashRegister.Queries.GetCashSessionDeposits;
using Devlivery.Features.CashRegister.Queries.GetCashSessions;
using Devlivery.Tests.Common.Api;
using Devlivery.Tests.Common.Builders;

using Microsoft.EntityFrameworkCore;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Api;

[Collection(ApiCollection.Name)]
[Trait("Category", "Integration Tests")]
public sealed class CashRegisterApiTests(ApiFactory api)
{
    [Fact]
    public async Task GivenNoOpenSession_WhenOpeningSession_ThenReturns201ForAuthenticatedOperator()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync("Maria Souza");

        // Act
        var response = await tenant.Client.PostAsJsonAsync("/api/cash-register/sessions",
            new CreateCashSessionCommand(100m, "Turno da noite"), ApiTenant.JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await ApiTenant.ReadDataAsync<CreateCashSessionResponse>(response);
        response.Headers.Location.ShouldBe(new Uri($"/api/cash-register/sessions/{created.Id}", UriKind.Relative));
        created.AttendantName.ShouldBe("Maria Souza");
        var persisted = await tenant.WithDbContextAsync(db => db.CashSessions.SingleAsync(s => s.Id == created.Id));
        persisted.AttendantId.ShouldBe(tenant.Operator.Id);
        persisted.OpeningAmount.ShouldBe(100m);
    }

    [Fact]
    public async Task GivenOpenSession_WhenOpeningAnother_ThenReturns422()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        await tenant.SeedAsync(OpenSession(tenant));

        // Act
        var response = await tenant.Client.PostAsJsonAsync("/api/cash-register/sessions",
            new CreateCashSessionCommand(100m, null), ApiTenant.JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task GivenOpenSession_WhenDepositing_ThenReturns201AndDepositIsListedWithOperatorName()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync("Maria Souza");
        var session = OpenSession(tenant);
        await tenant.SeedAsync(session);

        // Act
        var response = await tenant.Client.PostAsJsonAsync($"/api/cash-register/sessions/{session.Id}/deposits",
            new { Amount = 50m, Notes = "Reforço de troco" }, ApiTenant.JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await ApiTenant.ReadDataAsync<CreateCashDepositResponse>(response);
        var deposits = await ApiTenant.ReadDataAsync<GetCashSessionDepositsResponse[]>(
            await tenant.Client.GetAsync($"/api/cash-register/sessions/{session.Id}/deposits"));
        var deposit = deposits.ShouldHaveSingleItem();
        deposit.Id.ShouldBe(created.Id);
        deposit.Amount.ShouldBe(50m);
        deposit.AttendantId.ShouldBe(tenant.Operator.Id);
        deposit.AttendantName.ShouldBe("Maria Souza");
        deposit.Notes.ShouldBe("Reforço de troco");
    }

    [Fact]
    public async Task GivenOpenSession_WhenClosing_ThenReturns204AndSessionIsNoLongerActive()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var session = OpenSession(tenant);
        await tenant.SeedAsync(session);

        // Act
        var response = await tenant.Client.PatchAsJsonAsync($"/api/cash-register/sessions/{session.Id}/close",
            new { ClosingAmount = 180m, Notes = "Conferido" }, ApiTenant.JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await tenant.Client.GetAsync("/api/cash-register/sessions/active")).StatusCode
            .ShouldBe(HttpStatusCode.NotFound);
        var persisted = await tenant.WithDbContextAsync(db => db.CashSessions.SingleAsync(s => s.Id == session.Id));
        persisted.Status.ShouldBe(CashSessionStatus.Closed);
        persisted.ClosingAmount.ShouldBe(180m);
    }

    [Fact]
    public async Task GivenSessionWithMovements_WhenGettingActiveSession_ThenReturnsTotalsAndBreakdownByMethod()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var session = SessionWithMovements(tenant);
        await tenant.SeedAsync(session);

        // Act
        var response = await tenant.Client.GetAsync("/api/cash-register/sessions/active");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var active = await ApiTenant.ReadDataAsync<GetActiveCashSessionResponse>(response);
        active.Id.ShouldBe(session.Id);
        active.TotalRevenue.ShouldBe(90m);
        active.ExpectedCashAmount.ShouldBe(185m);
        active.PaymentBreakdown.Select(b => (b.Method, b.Amount)).ShouldBe(
            [(PaymentMethod.Cash, 60m), (PaymentMethod.Pix, 30m)], ignoreOrder: true);
    }

    [Fact]
    public async Task GivenSessionWithMovements_WhenGettingById_ThenListsCashMovementsWithoutPayments()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var session = SessionWithMovements(tenant);
        await tenant.SeedAsync(session);

        // Act
        var response = await tenant.Client.GetAsync($"/api/cash-register/sessions/{session.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await ApiTenant.ReadDataAsync<GetCashSessionByIdResponse>(response);
        body.TotalRevenue.ShouldBe(90m);
        body.CashMovements.Select(m => (m.EntryType, m.Amount)).ShouldBe(
            [(CashSessionEntryType.Change, 20m), (CashSessionEntryType.Deposit, 25m)], ignoreOrder: true);
    }

    [Fact]
    public async Task GivenSessionsWithMovements_WhenListing_ThenReturnsNewestFirstWithTheirTotals()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var closed = new CashSessionBuilder().WithEstablishmentId(tenant.EstablishmentId)
            .WithAttendantId(tenant.Operator.Id).WithOpeningAmount(10m).WithPayment(15m, PaymentMethod.Pix)
            .AsClosed(10m).Build();
        await tenant.SeedAsync(closed);
        var open = SessionWithMovements(tenant);
        await tenant.SeedAsync(open);

        // Act
        var response = await tenant.Client.GetAsync("/api/cash-register/sessions");

        // Assert
        var sessions = await ApiTenant.ReadDataAsync<GetCashSessionsResponse[]>(response);
        sessions.Select(s => s.Id).ShouldBe([open.Id, closed.Id]);
        sessions[0].TotalRevenue.ShouldBe(90m);
        sessions[0].ExpectedCashAmount.ShouldBe(185m);
        sessions[0].TotalOrders.ShouldBe(3);
        sessions[1].TotalRevenue.ShouldBe(15m);
    }

    [Fact]
    public async Task GivenOpenAndClosedSessions_WhenFilteringByStatus_ThenReturnsOnlyMatchingSessions()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var closed = new CashSessionBuilder().WithEstablishmentId(tenant.EstablishmentId)
            .WithAttendantId(tenant.Operator.Id).AsClosed().Build();
        var open = OpenSession(tenant);
        await tenant.SeedAsync(closed, open);

        // Act
        var response = await tenant.Client.GetAsync("/api/cash-register/sessions?status=Closed");

        // Assert
        var sessions = await ApiTenant.ReadDataAsync<GetCashSessionsResponse[]>(response);
        sessions.ShouldHaveSingleItem().Id.ShouldBe(closed.Id);
    }

    private static CashSession OpenSession(ApiTenant tenant) =>
        new CashSessionBuilder().WithEstablishmentId(tenant.EstablishmentId)
            .WithAttendantId(tenant.Operator.Id).WithOpeningAmount(100m).Build();

    // Opening 100 + deposit 25; cash payments 80 + 10 (refunded) and Pix 30; change of 20 on the first order.
    // Revenue: 120 - 10 - 20 = 90. Expected cash: 100 + 25 + (90 - 10 - 20) = 185.
    private static CashSession SessionWithMovements(ApiTenant tenant)
    {
        var cashOrder = Guid.NewGuid();
        var refundedPayment = Guid.NewGuid();
        return new CashSessionBuilder()
            .WithEstablishmentId(tenant.EstablishmentId)
            .WithAttendantId(tenant.Operator.Id)
            .WithOpeningAmount(100m)
            .WithPayment(80m, PaymentMethod.Cash, cashOrder)
            .WithPayment(30m, PaymentMethod.Pix)
            .WithPayment(10m, PaymentMethod.Cash, orderPaymentId: refundedPayment)
            .WithReversal(10m, PaymentMethod.Cash, refundedPayment)
            .WithChange(20m, cashOrder)
            .WithDeposit(25m)
            .Build();
    }
}