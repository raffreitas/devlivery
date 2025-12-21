using System.Net;
using System.Text.Json;

using Devlivery.Features.CashRegister.Commands.CreateCashSession;
using Devlivery.Tests.Common;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Commands.CreateCashDeposit;

[Collection("CashRegister Tests")]
[Trait("Category", "Integration Tests")]
public sealed class CreateCashDepositEndpointTests(CashRegisterWebApplicationFactory factory)
    : WebApiBaseFixture<CashRegisterWebApplicationFactory>(factory)
{
    [Fact]
    public async Task CreateCashDeposit_WithValidData_ReturnsCreatedAndDeposit()
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

        // Create deposit
        var attendantId = Guid.NewGuid();
        var attendantName = Faker.Name.FullName();
        var amount = Faker.Random.Decimal(10m, 500m);
        var notes = Faker.Lorem.Sentence();

        var command = new { AttendantId = attendantId, AttendantName = attendantName, Amount = amount, Notes = notes };

        // Act
        var response = await PostAsync($"/api/cash-register/sessions/{cashSessionId}/deposits", command, accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        responseData.RootElement.TryGetProperty("data", out var data).ShouldBeTrue();
        data.GetProperty("id").GetGuid().ShouldNotBe(Guid.Empty);
        data.GetProperty("attendantName").GetString().ShouldBe(attendantName);
        data.GetProperty("amount").GetDecimal().ShouldBe(amount);
    }

    [Fact]
    public async Task CreateCashDeposit_WithInvalidData_ReturnsValidationProblem()
    {
        // Arrange
        await ResetDatabaseAsync();

        var (_, _, accessToken) = await Prepare();
        var command = new { AttendantId = Guid.Empty, AttendantName = "", Amount = -10m, Notes = "" };

        // Act
        var response = await PostAsync($"/api/cash-register/sessions/{Guid.NewGuid()}/deposits", command, accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        responseData.RootElement.TryGetProperty("success", out var success).ShouldBeTrue();
        success.GetBoolean().ShouldBeFalse();
        responseData.RootElement.TryGetProperty("errors", out var errors).ShouldBeTrue();
        errors.ValueKind.ShouldBe(JsonValueKind.Array);
        errors.GetArrayLength().ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task CreateCashDeposit_WithNonExistentCashSession_ReturnsBadRequest()
    {
        // Arrange
        await ResetDatabaseAsync();

        var (_, _, accessToken) = await Prepare();
        var nonExistentCashSessionId = Guid.NewGuid();
        var command = new
        {
            AttendantId = Guid.NewGuid(), AttendantName = Faker.Name.FullName(), Amount = 50m, Notes = (string?)null
        };

        // Act
        var response = await PostAsync($"/api/cash-register/sessions/{nonExistentCashSessionId}/deposits", command,
            accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}