using Devlivery.Shared.Identity.Context;
using Devlivery.Shared.Persistence.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace Devlivery.Tests.Common;

public abstract class BaseWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>, IAsyncLifetime
    where TEntryPoint : class
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("devlivery")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithCleanUp(true)
        .Build();

    private Respawner _respawner = null!;
    private NpgsqlConnection _dbConnection = null!;

    private string ConnectionString => _postgresContainer.GetConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", ConnectionString);
        base.ConfigureWebHost(builder);
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        using var scope = Services.CreateScope();

        var appDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await appDbContext.Database.MigrateAsync();
        var identityDbContext = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();
        await identityDbContext.Database.MigrateAsync();

        _dbConnection = new NpgsqlConnection(ConnectionString);
        await _dbConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = ["__EFMigrationsHistory"]
        });
    }


    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbConnection.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }
}