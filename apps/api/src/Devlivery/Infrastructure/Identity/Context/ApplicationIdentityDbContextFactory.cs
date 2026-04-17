using Devlivery.Common.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Devlivery.Infrastructure.Identity.Context;

internal sealed class ApplicationIdentityDbContextFactory : IDesignTimeDbContextFactory<ApplicationIdentityDbContext>
{
    public ApplicationIdentityDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionStringOrThrow("DatabaseConnection");
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationIdentityDbContext>();
        optionsBuilder.UseNpgsql(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure();
        }).UseSnakeCaseNamingConvention();
        return new ApplicationIdentityDbContext(optionsBuilder.Options);
    }
}