using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Devlivery.Features.Auth.Commands.Login;
using Devlivery.Tests.Common.Api;

using Shouldly;

namespace Devlivery.Tests.Features.Auth.Api;

[Collection(ApiCollection.Name)]
[Trait("Category", "Integration Tests")]
public sealed class AuthApiTests(ApiFactory api)
{
    private const string Password = "Senha-Segura-123";

    [Fact]
    public async Task GivenValidCredentials_WhenLoggingIn_ThenReturnsTokenThatAuthorizesApiCalls()
    {
        // Arrange
        var email = $"{Guid.NewGuid():N}@pizzaria.com";
        var user = await api.CreateLoginUserAsync(email, Password);
        using var client = api.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginCommand(email, Password));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var login = await ApiTenant.ReadDataAsync<LoginResponse>(response);
        login.UserId.ShouldBe(user.Id);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        (await client.GetAsync("/api/v1/products")).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GivenWrongPassword_WhenLoggingIn_ThenReturns401WithGenericMessage()
    {
        // Arrange
        var email = $"{Guid.NewGuid():N}@pizzaria.com";
        await api.CreateLoginUserAsync(email, Password);
        using var client = api.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginCommand(email, "senha-errada"));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await ApiTenant.ReadProblemAsync(response)).Detail.ShouldBe("Usuário ou senha inválidos.");
    }

    [Fact]
    public async Task GivenRepeatedWrongPasswords_WhenLoggingInWithCorrectPassword_ThenAccountIsLockedOut()
    {
        // Arrange
        var email = $"{Guid.NewGuid():N}@pizzaria.com";
        await api.CreateLoginUserAsync(email, Password);
        using var client = api.CreateClient();
        for (var attempt = 0; attempt < 5; attempt++)
            await client.PostAsJsonAsync("/api/v1/auth/login", new LoginCommand(email, "senha-errada"));

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginCommand(email, Password));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}