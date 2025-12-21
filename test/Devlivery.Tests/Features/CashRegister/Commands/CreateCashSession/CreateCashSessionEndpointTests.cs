using System.Net;
using System.Text.Json;

using Devlivery.Features.CashRegister.Commands.CreateCashSession;
using Devlivery.Tests.Common;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Commands.CreateCashSession;

[Collection("CashRegister Tests")]
[Trait("Category", "Integration Tests")]
public sealed class CreateCashSessionEndpointTests(CashRegisterWebApplicationFactory factory)
    : WebApiBaseFixture<CashRegisterWebApplicationFactory>(factory)
{
    [Fact]
    public async Task CreateCashSession_WithValidData_ReturnsCreatedAndCashSession()
    {
        // Arrange
        await ResetDatabaseAsync();

        var (_, _, accessToken) = await Prepare();
        var attendantId = Guid.NewGuid();
        var attendantName = Faker.Name.FullName();
        var openingAmount = Faker.Random.Decimal(0m, 500m);
        var notes = Faker.Lorem.Sentence();

        var command = new CreateCashSessionCommand(attendantId, attendantName, openingAmount, notes);

        // Act
        var response = await PostAsync("/api/cash-register/sessions", command, accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        responseData.RootElement.TryGetProperty("data", out var data).ShouldBeTrue();
        data.GetProperty("id").GetGuid().ShouldNotBe(Guid.Empty);
        data.GetProperty("attendantName").GetString().ShouldBe(attendantName);
        data.GetProperty("openingAmount").GetDecimal().ShouldBe(openingAmount);
        data.GetProperty("status").GetString().ShouldBe("Open");
    }

    [Fact]
    public async Task CreateCashSession_WithInvalidData_ReturnsValidationProblem()
    {
        // Arrange
        await ResetDatabaseAsync();

        var (_, _, accessToken) = await Prepare();
        var command = new CreateCashSessionCommand(Guid.Empty, "", -10m, null);

        // Act
        var response = await PostAsync("/api/cash-register/sessions", command, accessToken);

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
    public async Task CreateCashSession_WhenActiveCashSessionExists_ReturnsBadRequest()
    {
        // Arrange
        await ResetDatabaseAsync();

        var (_, _, accessToken) = await Prepare();
        var attendantId = Guid.NewGuid();
        var attendantName = Faker.Name.FullName();
        const decimal openingAmount = 100m;

        var firstCommand = new CreateCashSessionCommand(attendantId, attendantName, openingAmount, null);
        await PostAsync("/api/cash-register/sessions", firstCommand, accessToken);

        var secondCommand = new CreateCashSessionCommand(attendantId, attendantName, openingAmount, null);

        // Act
        var response = await PostAsync("/api/cash-register/sessions", secondCommand, accessToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }
}