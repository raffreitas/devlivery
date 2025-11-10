using System.Net;
using System.Text.Json;
using Devlivery.WebApi.Features.Auth.Commands.Login;
using Devlivery.WebApi.Tests.Common;
using Shouldly;

namespace Devlivery.WebApi.Tests.Features.Auth.Commands.Login;

[Trait("Category", "Integration Tests")]
public sealed class LoginEndpointTests(CustomWebApplicationFactory factory) : WebApiBaseFixture(factory), IAsyncLifetime
{
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsJwtToken()
    {
        // Arrange
        var email = Faker.Internet.Email();
        const string password = "P@ssw0rd!";
        await CreateUserAsync(email: email, password: password);

        // Act
        var response = await PostAsync("/api/auth/login", new LoginCommand(email, password));

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
        var email = Faker.Internet.Email();
        const string password = "P@ssw0rd!";
        await CreateUserAsync(email: email, password: password);
        const string invalidPassword = "WrongP@ssw0rd!";

        // Act
        var response = await PostAsync("/api/auth/login", new LoginCommand(email, invalidPassword));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await CleanUpDatabaseAsync();
}