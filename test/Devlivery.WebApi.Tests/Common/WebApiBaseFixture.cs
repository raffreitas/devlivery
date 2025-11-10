using System.Net.Http.Headers;
using System.Net.Http.Json;
using Bogus;
using Devlivery.WebApi.Features.Auth.Abstractions;
using Devlivery.WebApi.Features.Users.Domain;
using Devlivery.WebApi.Shared.Database.Context;
using Devlivery.WebApi.Shared.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Devlivery.WebApi.Tests.Common;

/// <summary>
/// Base class for integration tests with helper methods.
/// Each test should call ResetDatabaseAsync() in constructor or setup.
/// </summary>
public abstract class WebApiBaseFixture<TFactory>(TFactory factory)
    where TFactory : BaseWebApplicationFactory<Program>
{
    protected static Faker Faker => new();
    protected readonly TFactory Factory = factory;

    private readonly HttpClient _httpClient = factory.CreateClient();

    protected async Task<HttpResponseMessage> PostAsync<T>(string method, T request, string? token = "")
    {
        if (!string.IsNullOrWhiteSpace(token))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await _httpClient.PostAsJsonAsync(method, request);
    }

    protected async Task<HttpResponseMessage> PutAsync<T>(string method, T request, string? token = "")
    {
        if (!string.IsNullOrWhiteSpace(token))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await _httpClient.PutAsJsonAsync(method, request);
    }

    protected async Task<HttpResponseMessage> PatchAsync<T>(string method, T request, string? token = "")
    {
        if (!string.IsNullOrWhiteSpace(token))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var msg = new HttpRequestMessage(new HttpMethod("PATCH"), method)
        {
            Content = JsonContent.Create(request)
        };

        return await _httpClient.SendAsync(msg);
    }

    protected async Task<HttpResponseMessage> GetAsync(string method, string? token = "")
    {
        if (!string.IsNullOrWhiteSpace(token))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await _httpClient.GetAsync(method);
    }

    protected async Task<HttpResponseMessage> DeleteAsync(string method, string? token = "")
    {
        if (!string.IsNullOrWhiteSpace(token))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await _httpClient.DeleteAsync(method);
    }

    protected async Task ResetDatabaseAsync()
    {
        await Factory.ResetDatabaseAsync();
    }

    protected async Task<User> CreateUserAsync(string? name = null, string? email = null, string? password = null)
    {
        name ??= Faker.Name.FullName();
        email ??= Faker.Internet.Email();
        password ??= Faker.Internet.Password(length: 5, prefix: "P@ssw0rd1");

        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new User(name, email);

        var identityResult = await userManager.CreateAsync(new ApplicationUser
        {
            UserId = user.Id,
            UserName = user.Email,
            Email = user.Email,
            EmailConfirmed = true,
        }, password);

        if (!identityResult.Succeeded)
            throw new InvalidOperationException("Failed to create user in identity store.");

        var appDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await appDbContext.Users.AddAsync(user);
        await appDbContext.SaveChangesAsync();

        return user;
    }

    protected async Task<string> GetAccessTokenAsync(User? user = null)
    {
        user ??= await CreateUserAsync();

        using var scope = Factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var tokenRequest = new TokenRequest(user.Id.ToString(), user.Email);
        var token = await tokenService.GenerateTokenAsync(tokenRequest);
        return token;
    }
}