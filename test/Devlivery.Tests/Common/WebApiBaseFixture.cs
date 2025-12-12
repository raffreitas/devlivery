using System.Net.Http.Headers;
using System.Net.Http.Json;

using Bogus;

using Devlivery.Features.Establishments.Domain;
using Devlivery.Features.Users.Domain;
using Devlivery.Shared.Infrastructure.Identity.Abstractions;
using Devlivery.Shared.Infrastructure.Identity.Users.Models;
using Devlivery.Shared.Infrastructure.Persistence.Context;
using Devlivery.Tests.Common.Builders;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Devlivery.Tests.Common;

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
        using var msg = new HttpRequestMessage(HttpMethod.Post, method);

        if (!string.IsNullOrWhiteSpace(token))
            msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        msg.Content = JsonContent.Create(request);

        return await _httpClient.SendAsync(msg);
    }

    protected async Task<HttpResponseMessage> PutAsync<T>(string method, T request, string? token = "")
    {
        using var msg = new HttpRequestMessage(HttpMethod.Put, method);

        if (!string.IsNullOrWhiteSpace(token))
            msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        msg.Content = JsonContent.Create(request);

        return await _httpClient.SendAsync(msg);
    }

    protected async Task<HttpResponseMessage> PatchAsync<T>(string method, T request, string? token = "")
    {
        using var msg = new HttpRequestMessage(HttpMethod.Patch, method);

        if (!string.IsNullOrWhiteSpace(token))
            msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        msg.Content = JsonContent.Create(request);

        return await _httpClient.SendAsync(msg);
    }

    protected async Task<HttpResponseMessage> GetAsync(string method, string? token = "")
    {
        using var msg = new HttpRequestMessage(HttpMethod.Get, method);

        if (!string.IsNullOrWhiteSpace(token))
            msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await _httpClient.SendAsync(msg);
    }

    protected async Task<HttpResponseMessage> DeleteAsync(string method, string? token = "")
    {
        using var msg = new HttpRequestMessage(HttpMethod.Delete, method);

        if (!string.IsNullOrWhiteSpace(token))
            msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await _httpClient.SendAsync(msg);
    }

    protected async Task ResetDatabaseAsync()
    {
        await Factory.ResetDatabaseAsync();
    }

    protected async Task<(User user, Establishment establishment, string accessToken)> Prepare(
        User? user = null,
        Establishment? establishment = null,
        string? password = null)
    {
        establishment ??= new EstablishmentBuilder().Build();
        user ??= new UserBuilder()
            .WithEstablishmentId(establishment.Id)
            .Build();
        password ??= Faker.Internet.Password(length: 5, prefix: "P@ssw0rd1");

        using var scope = Factory.Services.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var appDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        var identityResult = await userManager.CreateAsync(new ApplicationUser
        {
            UserId = user.Id,
            UserName = user.Email,
            Email = user.Email,
            EmailConfirmed = true,
        }, password);

        if (!identityResult.Succeeded)
            throw new InvalidOperationException("Failed to create user in identity store.");

        await appDbContext.Establishments.AddAsync(establishment);
        await appDbContext.Users.AddAsync(user);
        await appDbContext.SaveChangesAsync();

        var token = await tokenService.GenerateTokenAsync(new TokenRequest(
            user.Id.ToString(),
            user.EstablishmentId.ToString()));

        return (user, establishment, token);
    }
}