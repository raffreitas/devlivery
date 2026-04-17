using Devlivery.Common.Extensions;
using Devlivery.Infrastructure.Persistence.Context;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Devlivery.Infrastructure.Persistence.Factory;

internal sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionStringOrThrow("DatabaseConnection");
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure();
        }).UseSnakeCaseNamingConvention();
        return new ApplicationDbContext(optionsBuilder.Options, null!);
    }
}