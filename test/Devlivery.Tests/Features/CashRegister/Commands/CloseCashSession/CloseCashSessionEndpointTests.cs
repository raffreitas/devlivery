using System.Net;
using System.Text.Json;
using Devlivery.Features.CashRegister.Commands.CloseCashSession;
using Devlivery.Features.CashRegister.Commands.CreateCashSession;
using Devlivery.Tests.Common;
using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Commands.CloseCashSession;

[Collection("CashRegister Tests")]
[Trait("Category", "Integration Tests")]
public sealed class CloseCashSessionEndpointTests(CashRegisterWebApplicationFactory factory)
    : WebApiBaseFixture<CashRegisterWebApplicationFactory>(factory)
{
    [Fact]
    public async Task CloseCashSession_WithValidData_ReturnsOkAndClosedSession()
    {
        // Arrange
        await ResetDatabaseAsync();

        var (_, _, accessToken) = await Prepare();

        // First, create a cash session
        var sessionCommand = new CreateCashSessionCommand(
            Guid.NewGuid(),
            Faker.Name.FullName(),
            100m,
            null);
        var sessionResponse = await PostAsync("/api/cash-register/sessions", sessionCommand, accessToken);
        await using var sessionBody = await sessionResponse.Content.ReadAsStreamAsync();
        var sessionData = await JsonDocument.ParseAsync(sessionBody);
        var cashSessionId = sessionData.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        // Close session
        const decimal closingAmount = 250m;
        var notes = Faker.Lorem.Sentence();
        var command = new CloseCashSessionCommand(cashSessionId, closingAmount, notes);

        // Act
        var response = await PatchAsync($"/api/cash-register/sessions/{cashSessionId}/close", command, accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        responseData.RootElement.TryGetProperty("data", out var data).ShouldBeTrue();
        data.GetProperty("id").GetGuid().ShouldBe(cashSessionId);
        data.GetProperty("status").GetString().ShouldBe("Closed");
        data.GetProperty("closingAmount").GetDecimal().ShouldBe(closingAmount);
    }

    [Fact]
    public async Task CloseCashSession_WithInvalidData_ReturnsUnprocessableEntity()
    {
        // Arrange
        await ResetDatabaseAsync();

        var (_, _, accessToken) = await Prepare();
        var cashSessionId = Guid.Empty;
        var command = new CloseCashSessionCommand(cashSessionId, -10m, null);

        // Act
        var response = await PatchAsync($"/api/cash-register/sessions/{cashSessionId}/close", command, accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        responseData.RootElement.TryGetProperty("success", out var success).ShouldBeTrue();
        success.GetBoolean().ShouldBeFalse();
    }

    [Fact]
    public async Task CloseCashSession_WithNonExistentSession_ReturnsNotFound()
    {
        // Arrange
        await ResetDatabaseAsync();

        var (_, _, accessToken) = await Prepare();
        var cashSessionId = Guid.NewGuid();
        var command = new CloseCashSessionCommand(cashSessionId, 100m, null);

        // Act
        var response = await PatchAsync($"/api/cash-register/sessions/{cashSessionId}/close", command, accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CloseCashSession_WhenAlreadyClosed_ReturnsBadRequest()
    {
        // Arrange
        await ResetDatabaseAsync();

        var (_, _, accessToken) = await Prepare();

        // Create and close a cash session
        var sessionCommand = new CreateCashSessionCommand(
            Guid.NewGuid(),
            Faker.Name.FullName(),
            100m,
            null);
        var sessionResponse = await PostAsync("/api/cash-register/sessions", sessionCommand, accessToken);
        await using var sessionBody = await sessionResponse.Content.ReadAsStreamAsync();
        var sessionData = await JsonDocument.ParseAsync(sessionBody);
        var cashSessionId = sessionData.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        var firstCloseCommand = new CloseCashSessionCommand(cashSessionId, 100m, null);
        await PatchAsync($"/api/cash-register/sessions/{cashSessionId}/close", firstCloseCommand, accessToken);

        // Try to close again
        var secondCloseCommand = new CloseCashSessionCommand(cashSessionId, 100m, null);

        // Act
        var response = await PatchAsync($"/api/cash-register/sessions/{cashSessionId}/close", secondCloseCommand,
            accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }
}