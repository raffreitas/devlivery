using System.Globalization;
using System.Net;
using System.Net.Http.Json;

using Devlivery.Common.Time;
using Devlivery.Domain.Aggregates.CashRegister;
using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Domain.Common.Enums;
using Devlivery.Features.CashRegister.Commands.CreateCashWithdrawal;
using Devlivery.Features.CashRegister.Queries;
using Devlivery.Features.CashRegister.Queries.GetCashHistory;
using Devlivery.Features.CashRegister.Queries.GetCashMovements;
using Devlivery.Features.CashRegister.Queries.GetCashOverview;
using Devlivery.Tests.Common.Api;
using Devlivery.Tests.Common.Builders;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Api;

[Collection(ApiCollection.Name)]
[Trait("Category", "Integration Tests")]
public sealed class CashWithdrawalApiTests(ApiFactory api)
{
    [Fact]
    public async Task GivenHeaderKey_WhenReplayingWithdrawal_ThenReturnsTheSameMovement()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var session = NewSession(tenant);
        await tenant.SeedAsync(session);
        var requestId = Guid.NewGuid();
        var request = new WithdrawalInput(30m, "Cofre");

        // Act
        var created = await Post(tenant, session, requestId, request);
        var replay = await Post(tenant, session, requestId, request);
        var detail = await ApiTenant.ReadDataAsync<CashSessionSummary>(
            await tenant.Client.GetAsync($"/api/v2/cash-register/sessions/{session.Id}"));

        // Assert
        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        replay.StatusCode.ShouldBe(HttpStatusCode.OK);
        var movement = await ApiTenant.ReadDataAsync<CashWithdrawalResponse>(created);
        (await ApiTenant.ReadDataAsync<CashWithdrawalResponse>(replay)).ShouldBe(movement);
        detail.WithdrawalsCount.ShouldBe(1);
        detail.ExpectedCashAmount.ShouldBe(70m);
    }

    [Fact]
    public async Task GivenMissingInvalidOrRepeatedHeader_WhenWithdrawing_ThenReturnsValidationWithoutConsumingTheKey()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var session = NewSession(tenant);
        await tenant.SeedAsync(session);
        var requestId = Guid.NewGuid();
        var request = new WithdrawalInput(30m, "Cofre");
        var header = requestId.ToString();

        // Act
        var missing = await PostWithHeader(tenant, session, request);
        var bodyOnly = await PostWithHeader(tenant, session, new { requestId, request.Amount, request.Reason });
        var malformed = await PostWithHeader(tenant, session, request, "not-a-uuid");
        var duplicate = await PostWithHeader(tenant, session, request, header, header);
        var empty = await PostWithHeader(tenant, session, request, Guid.Empty.ToString());
        var created = await Post(tenant, session, requestId, request);

        // Assert
        foreach (var response in new[] { missing, bodyOnly, malformed, duplicate, empty })
        {
            response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
            (await ApiTenant.ReadProblemAsync(response)).Errors.ShouldNotBeNull().ShouldContainKey("requestId");
        }
        created.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GivenMixedPayments_WhenRegisteringAndReplayingWithdrawal_ThenLedgerAndClosedHistoryReconcile()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync("Ana Lima");
        var colleague = new UserBuilder().WithEstablishmentId(tenant.EstablishmentId).Build();
        var session = NewSession(tenant);
        var orderId = Guid.NewGuid();
        session.AddPayment(Guid.NewGuid(), 80m, PaymentMethod.Cash, orderId, tenant.Operator.Id);
        session.AddPayment(Guid.NewGuid(), 30m, PaymentMethod.Pix, orderId, tenant.Operator.Id);
        session.AddChange(orderId, 10m, tenant.Operator.Id);
        await tenant.SeedAsync(session, colleague);
        var sameEstablishment = new ApiTenant(api, tenant.EstablishmentId, colleague);
        await sameEstablishment.AuthenticateAsync();
        var requestId = Guid.NewGuid();
        var request = new WithdrawalInput(120m, "  Guardado no cofre ");

        // Act
        var response = await Post(tenant, session, requestId, request);
        var movement = await ApiTenant.ReadDataAsync<CashWithdrawalResponse>(response);
        var overview = await ApiTenant.ReadDataAsync<GetCashOverviewResponse>(
            await tenant.Client.GetAsync("/api/v2/cash-register/overview"));
        var entries = await ApiTenant.ReadDataAsync<GetCashMovementsResponse[]>(
            await tenant.Client.GetAsync($"/api/v2/cash-register/sessions/{session.Id}/movements"));
        var close = await Close(tenant, session, 49m);
        var replay = await Post(tenant, session, requestId, request with { Amount = 120.00m, Reason = "Guardado no cofre" });
        var changedReplay = await Post(tenant, session, requestId, request with { Amount = 119m });
        var invalidReplay = await Post(tenant, session, requestId, request with { Amount = 0m });
        var differentActor = await Post(sameEstablishment, session, requestId, request);
        var newRequest = await Post(tenant, session, Guid.NewGuid(), request);
        var detail = await ApiTenant.ReadDataAsync<CashSessionSummary>(
            await tenant.Client.GetAsync($"/api/v2/cash-register/sessions/{session.Id}"));
        var date = session.StartAt.ToBrazilTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var history = await ApiTenant.ReadDataAsync<GetCashHistoryResponse>(
            await tenant.Client.GetAsync($"/api/v2/cash-register/sessions?start={date}&end={date}"));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        movement.Amount.ShouldBe(120m);
        movement.Reason.ShouldBe("Guardado no cofre");
        movement.CreatedBy.ShouldBe(tenant.Operator.Id);
        movement.CreatedAt.Kind.ShouldBe(DateTimeKind.Utc);
        var active = overview.ActiveSession.ShouldNotBeNull();
        active.ExpectedCashAmount.ShouldBe(50m);
        active.TotalRevenue.ShouldBe(100m);
        active.TotalOrders.ShouldBe(1);
        active.PaymentBreakdown.Sum(p => p.Amount).ShouldBe(100m);
        active.WithdrawalsTotal.ShouldBe(120m);
        active.WithdrawalsCount.ShouldBe(1);
        var entry = entries.Single(e => e.Type == "Withdrawal");
        entry.Id.ShouldBe(movement.Id.ToString());
        entry.SignedAmount.ShouldBe(-120m);
        entry.Method.ShouldBe(PaymentMethod.Cash);
        entry.AuthorName.ShouldBe("Ana Lima");
        entry.CreatedAt.ShouldBe(movement.CreatedAt);
        close.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        replay.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await ApiTenant.ReadDataAsync<CashWithdrawalResponse>(replay)).ShouldBe(movement);
        changedReplay.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        invalidReplay.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        differentActor.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        newRequest.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        detail.Difference.ShouldBe(-1m);
        detail.WithdrawalsCount.ShouldBe(1);
        history.Sessions.ShouldHaveSingleItem().Difference.ShouldBe(detail.Difference);
        history.Summary.TotalDifference.ShouldBe(-1m);
        history.Summary.TotalRevenue.ShouldBe(100m);
    }

    [Fact]
    public async Task GivenInvalidRequestsAndAnotherEstablishment_WhenWithdrawing_ThenOnlyTheAvailableAmountIsAccepted()
    {
        // Arrange
        var owner = await api.CreateTenantAsync();
        var other = await api.CreateTenantAsync();
        var session = NewSession(owner);
        await owner.SeedAsync(session);
        var requestId = Guid.NewGuid();
        var request = new WithdrawalInput(100m, "Cofre");

        // Act
        var invalid = await Task.WhenAll(
            Post(owner, session, requestId, request with { Amount = 100.01m }),
            Post(owner, session, requestId, request with { Amount = 10.001m }),
            Post(owner, session, requestId, request with { Amount = 0m }),
            Post(owner, session, requestId, request with { Reason = "   " }),
            Post(owner, session, requestId, request with { Reason = new string('a', 1001) }));
        var exact = await Post(owner, session, requestId, request);
        var denied = await Post(other, session, requestId, request);
        var detail = await ApiTenant.ReadDataAsync<CashSessionSummary>(
            await owner.Client.GetAsync($"/api/v2/cash-register/sessions/{session.Id}"));

        // Assert
        invalid.ShouldAllBe(r => r.StatusCode == HttpStatusCode.UnprocessableEntity);
        var problem = await ApiTenant.ReadProblemAsync(invalid[0]);
        problem.Errors.ShouldNotBeNull()["amount"].ShouldBe("O valor da sangria supera o dinheiro disponível.");
        denied.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exact.StatusCode.ShouldBe(HttpStatusCode.Created);
        detail.ExpectedCashAmount.ShouldBe(0m);
        detail.WithdrawalsCount.ShouldBe(1);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GivenConcurrentWithdrawals_WhenRequestsOverlap_ThenOnlyOneWithdrawalIsRecorded(bool sameRequest)
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var session = NewSession(tenant);
        await tenant.SeedAsync(session);
        var request = new WithdrawalInput(80m, "Cofre");
        var firstKey = Guid.NewGuid();
        var secondKey = sameRequest ? firstKey : Guid.NewGuid();

        // Act
        var responses = await Task.WhenAll(Post(tenant, session, firstKey, request), Post(tenant, session, secondKey, request));
        var movements = await tenant.WithDbContextAsync(db => db.CashSessionMovements
            .Where(m => m.CashSessionId == session.Id && m.EntryType == CashSessionEntryType.Withdrawal).ToListAsync());

        // Assert
        movements.ShouldHaveSingleItem().Amount.ShouldBe(80m);
        responses.Count(r => r.StatusCode == HttpStatusCode.Created).ShouldBe(1);
        var other = responses.Single(r => r.StatusCode != HttpStatusCode.Created).StatusCode;
        if (sameRequest)
            other.ShouldBe(HttpStatusCode.OK);
        else
            new[] { HttpStatusCode.Conflict, HttpStatusCode.UnprocessableEntity }.ShouldContain(other);
    }

    [Fact]
    public async Task GivenConcurrentWithdrawalAndClose_WhenOneChangesTheSession_ThenClosedTotalsMatchTheLedger()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var session = NewSession(tenant);
        await tenant.SeedAsync(session);
        var request = new WithdrawalInput(40m, "Cofre");

        // Act
        var withdrawal = Post(tenant, session, Guid.NewGuid(), request);
        var close = Close(tenant, session, 60m);
        await Task.WhenAll(withdrawal, close);
        var closeResponse = await close;
        if (closeResponse.StatusCode == HttpStatusCode.Conflict)
            closeResponse = await Close(tenant, session, 60m);
        var detail = await ApiTenant.ReadDataAsync<CashSessionSummary>(
            await tenant.Client.GetAsync($"/api/v2/cash-register/sessions/{session.Id}"));

        // Assert
        var withdrawalStatus = (await withdrawal).StatusCode;
        new[] { HttpStatusCode.Created, HttpStatusCode.Conflict }.ShouldContain(withdrawalStatus);
        closeResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var recorded = withdrawalStatus == HttpStatusCode.Created;
        detail.Status.ShouldBe("Closed");
        detail.WithdrawalsCount.ShouldBe(recorded ? 1 : 0);
        detail.Difference.ShouldBe(recorded ? 0m : -40m);
    }

    [Fact]
    public async Task GivenLegacyWithdrawalKey_WhenMigrating_ThenTheApiStillReplaysTheSameMovement()
    {
        // Arrange
        await using var isolatedApi = new ApiFactory();
        await isolatedApi.InitializeAsync();
        var tenant = await isolatedApi.CreateTenantAsync();
        var session = NewSession(tenant);
        await tenant.SeedAsync(session);
        var requestId = Guid.NewGuid();
        var request = new WithdrawalInput(17.50m, "Organização do caixa\nCofre");
        var created = await Post(tenant, session, requestId, request);
        var movement = await ApiTenant.ReadDataAsync<CashWithdrawalResponse>(created);

        // Act
        await tenant.WithDbContextAsync(async db =>
        {
            var migrator = db.GetService<IMigrator>();
            await migrator.MigrateAsync("20261120142603_AddCashWithdrawalRequestId");
            var legacyKey = await db.Database.SqlQuery<Guid>(
                $"SELECT request_id AS \"Value\" FROM public.cash_session_movements WHERE id = {movement.Id}").SingleAsync();
            legacyKey.ShouldBe(requestId);
            await migrator.MigrateAsync();
        });
        var replay = await Post(tenant, session, requestId, request with { Amount = 17.5m });

        // Assert
        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        replay.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await ApiTenant.ReadDataAsync<CashWithdrawalResponse>(replay)).ShouldBe(movement);
    }

    private static CashSession NewSession(ApiTenant tenant) => new CashSessionBuilder()
        .WithEstablishmentId(tenant.EstablishmentId).WithAttendantId(tenant.Operator.Id)
        .WithAttendantName(tenant.Operator.Name).WithOpeningAmount(100m).Build();

    private static Task<HttpResponseMessage> Post(ApiTenant tenant, CashSession session, Guid requestId, WithdrawalInput input) =>
        PostWithHeader(tenant, session, input, requestId.ToString());

    private static async Task<HttpResponseMessage> PostWithHeader(
        ApiTenant tenant, CashSession session, object body, params string[] keys)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/cash-register/sessions/{session.Id}/withdrawals")
        {
            Content = JsonContent.Create(body)
        };
        if (keys.Length > 0) request.Headers.Add("Idempotency-Key", keys);
        return await tenant.Client.SendAsync(request);
    }

    private static Task<HttpResponseMessage> Close(ApiTenant tenant, CashSession session, decimal amount) =>
        tenant.Client.PatchAsJsonAsync($"/api/v1/cash-register/sessions/{session.Id}/close", new { closingAmount = amount });

    private sealed record WithdrawalInput(decimal Amount, string Reason);
}
