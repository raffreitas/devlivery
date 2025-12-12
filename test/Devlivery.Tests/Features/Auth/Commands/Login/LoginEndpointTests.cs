using System.Net;
using System.Text.Json;
using Devlivery.Features.Auth.Commands.Login;
using Devlivery.Tests.Common;
using Shouldly;

namespace Devlivery.Tests.Features.Auth.Commands.Login;

[Collection("Auth Tests")]
[Trait("Category", "Integration Tests")]
public sealed class LoginEndpointTests(AuthWebApplicationFactory factory)
    : WebApiBaseFixture<AuthWebApplicationFactory>(factory)
{
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsJwtToken()
    {
        // Arrange
        await ResetDatabaseAsync();

        const string password = "P@ssw0rd!";
        var (user, _, _) = await Prepare(password: password);

        // Act
        var response = await PostAsync("/api/auth/login", new LoginCommand(user.Email, password));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        responseData.ShouldNotBeNull();
        var apiResponse = responseData.RootElement.GetProperty("data");
        apiResponse.GetProperty("token").GetString().ShouldNotBeNull();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        await ResetDatabaseAsync();

        const string password = "P@ssw0rd!";
        const string invalidPassword = "WrongP@ssw0rd!";
        var (user, _, _) = await Prepare(password: password);

        // Act
        var response = await PostAsync("/api/auth/login", new LoginCommand(user.Email, invalidPassword));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}