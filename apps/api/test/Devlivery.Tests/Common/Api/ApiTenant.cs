using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using Devlivery.Domain.Aggregates.Users;
using Devlivery.Infrastructure.Http.Models;
using Devlivery.Infrastructure.Identity.Abstractions;
using Devlivery.Infrastructure.Persistence.Context;
using Devlivery.Infrastructure.Tenancy;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shouldly;

namespace Devlivery.Tests.Common.Api;

/// <summary>An establishment, its operator and an HTTP client authenticated as that operator.</summary>
public sealed class ApiTenant(ApiFactory factory, Guid establishmentId, User @operator)
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public Guid EstablishmentId { get; } = establishmentId;
    public User Operator { get; } = @operator;
    public HttpClient Client { get; } = factory.CreateClient();

    public async Task AuthenticateAsync()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var token = await scope.ServiceProvider.GetRequiredService<ITokenService>()
            .GenerateTokenAsync(new TokenRequest(Operator.Id.ToString(), EstablishmentId.ToString()));
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>Persists entities directly, bypassing the API, to arrange a scenario.</summary>
    public Task SeedAsync(params object[] entities) => WithDbContextAsync(async dbContext =>
    {
        dbContext.AddRange(entities);
        await dbContext.SaveChangesAsync();
    });

    /// <summary>Runs <paramref name="action"/> against the database as this tenant, e.g. to assert persisted state.</summary>
    public async Task WithDbContextAsync(Func<ApplicationDbContext, Task> action)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<ITenantAccessor>().Register(new Tenant(EstablishmentId));
        await action(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
    }

    public async Task<T> WithDbContextAsync<T>(Func<ApplicationDbContext, Task<T>> query)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<ITenantAccessor>().Register(new Tenant(EstablishmentId));
        return await query(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
    }

    /// <summary>Adjusts columns the domain controls (e.g. <c>created_at</c>) to arrange time-based scenarios.</summary>
    public Task ExecuteSqlAsync(FormattableString sql) =>
        WithDbContextAsync(dbContext => dbContext.Database.ExecuteSqlInterpolatedAsync(sql));

    public static async Task<T> ReadDataAsync<T>(HttpResponseMessage response)
    {
        var resource = await response.Content.ReadFromJsonAsync<ApiResource<T>>(JsonOptions);
        return resource.ShouldNotBeNull().Data;
    }

    public static async Task<ApiProblemDetails> ReadProblemAsync(HttpResponseMessage response)
    {
        var problem = await response.Content.ReadFromJsonAsync<ApiProblemDetails>(JsonOptions);
        return problem.ShouldNotBeNull();
    }
}