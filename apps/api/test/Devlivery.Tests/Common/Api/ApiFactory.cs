using Devlivery.Domain.Aggregates.Establishments;
using Devlivery.Domain.Aggregates.Users;
using Devlivery.Infrastructure.Identity.Context;
using Devlivery.Infrastructure.Identity.Users.Models;
using Devlivery.Infrastructure.Persistence.Context;
using Devlivery.Tests.Common.Builders;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Testcontainers.PostgreSql;

namespace Devlivery.Tests.Common.Api;

/// <summary>
/// Runs the real API (pipeline, authentication, tenancy, handlers and PostgreSQL) for HTTP contract tests.
/// One database is shared by the whole collection; tests isolate their data by creating their own tenant.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:16-alpine")
        .WithCleanUp(true)
        .Build();

    public async Task InitializeAsync()
    {
        await _database.StartAsync();

        await using var scope = Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
        await scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>().Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", _database.GetConnectionString());
    }

    /// <summary>Creates an establishment with an operator and returns a client authenticated as that operator.</summary>
    public async Task<ApiTenant> CreateTenantAsync(string operatorName = "Operador Teste")
    {
        var establishment = new EstablishmentBuilder().Build();
        var user = new UserBuilder().WithName(operatorName).WithEstablishmentId(establishment.Id).Build();

        var tenant = new ApiTenant(this, establishment.Id, user);
        await tenant.SeedAsync(establishment, user);
        await tenant.AuthenticateAsync();
        return tenant;
    }

    /// <summary>Creates an operator that can sign in through <c>POST /api/auth/login</c>.</summary>
    public async Task<User> CreateLoginUserAsync(string email, string password)
    {
        var establishment = new EstablishmentBuilder().Build();
        var user = new UserBuilder().WithEmail(email).WithEstablishmentId(establishment.Id).Build();

        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.AddRange(establishment, user);
        await dbContext.SaveChangesAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var created = await userManager.CreateAsync(
            new ApplicationUser { UserId = user.Id, UserName = email, Email = email, EmailConfirmed = true },
            password);
        if (!created.Succeeded)
            throw new InvalidOperationException(string.Join("; ", created.Errors.Select(e => e.Description)));

        return user;
    }
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "API";
}