using System.Net.Http.Headers;
using System.Net.Http.Json;
using Bogus;
using Devlivery.WebApi.Features.Auth.Abstractions;
using Devlivery.WebApi.Features.Users.Domain;
using Devlivery.WebApi.Shared.Database.Context;
using Devlivery.WebApi.Shared.Identity.Context;
using Devlivery.WebApi.Shared.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Devlivery.WebApi.Tests.Common;

[Collection("Integration Tests")]
public abstract class WebApiBaseFixture
{
    protected static Faker Faker => new();
    protected readonly ApplicationDbContext AppDbContext;

    private readonly ApplicationIdentityDbContext _identityDbContext;
    private readonly IServiceScope _serviceScope;
    private readonly HttpClient _httpClient;

    protected WebApiBaseFixture(CustomWebApplicationFactory factory)
    {
        _httpClient = factory.CreateClient();
        _serviceScope = factory.Services.CreateScope();
        AppDbContext = _serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _identityDbContext = _serviceScope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();

        if (AppDbContext.Database.GetPendingMigrations().Any())
            AppDbContext.Database.Migrate();
        if (_identityDbContext.Database.GetPendingMigrations().Any())
            _identityDbContext.Database.Migrate();
    }

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

    protected async Task CleanUpDatabaseAsync()
    {
        await AppDbContext.Database.EnsureDeletedAsync();
        await _identityDbContext.Database.EnsureDeletedAsync();
    }

    protected async Task<User> CreateUserAsync(string? name = null, string? email = null, string? password = null)
    {
        name ??= Faker.Name.FullName();
        email ??= Faker.Internet.Email();
        password ??= Faker.Internet.Password(length: 5, prefix: "P@ssw0rd1");

        var userManager = _serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            CreatedAt = DateTime.UtcNow,
        };

        var identityResult = await userManager.CreateAsync(new ApplicationUser
        {
            UserId = user.Id,
            UserName = user.Email,
            Email = user.Email,
            EmailConfirmed = true,
        }, password);

        if (!identityResult.Succeeded)
            throw new InvalidOperationException("Failed to create user in identity store.");

        await AppDbContext.Users.AddAsync(user);
        await AppDbContext.SaveChangesAsync();

        return user;
    }

    protected async Task<string> GetAccessTokenAsync(User? user = null)
    {
        user ??= await CreateUserAsync();
        var tokenService = _serviceScope.ServiceProvider.GetRequiredService<ITokenService>();
        var tokenRequest = new TokenRequest(user.Id.ToString(), user.Email);
        var token = await tokenService.GenerateTokenAsync(tokenRequest);
        return token;
    }
}