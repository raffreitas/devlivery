using Devlivery.Infrastructure.Persistence.Context;
using Devlivery.Infrastructure.Tenancy;

using Microsoft.EntityFrameworkCore;

using Testcontainers.PostgreSql;

namespace Devlivery.Tests.Infrastructure.Persistence;

/// <summary>
/// PostgreSQL container with the application migrations applied, shared by the tests of a class.
/// Tests isolate their data by using their own establishment (tenant).
/// </summary>
public sealed class PostgresDatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithCleanUp(true)
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var dbContext = new ApplicationDbContext(CreateOptions(), new TenantAccessor());
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    public DbContextOptions<ApplicationDbContext> CreateOptions() =>
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;
}