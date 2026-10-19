using System.Text;

using Devlivery.BackupJob.Services;
using Devlivery.BackupJob.Tests.Common;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Npgsql;

using NSubstitute;

using Shouldly;

namespace Devlivery.BackupJob.Tests.Services;

[Trait("Category", "Integration Tests")]
public sealed class PostgresDumpExporterTests(PostgresDatabaseFixture database)
    : IClassFixture<PostgresDatabaseFixture>, IDisposable
{
    private readonly string _outputDirectory = Path.Combine(Path.GetTempPath(), $"devlivery-backup-tests-{Guid.NewGuid():N}");

    [PgDumpFact]
    public async Task GivenReachableDatabase_WhenExporting_ThenWritesCustomFormatDump()
    {
        // Arrange
        var exporter = CreateExporter(database.ConnectionString);
        var outputFilePath = Path.Combine(_outputDirectory, "backup.dump");

        // Act
        var result = await exporter.ExportAsync(outputFilePath, CancellationToken.None);

        // Assert
        result.FilePath.ShouldBe(outputFilePath);
        result.DatabaseName.ShouldBe(new NpgsqlConnectionStringBuilder(database.ConnectionString).Database);
        result.SizeInBytes.ShouldBe(new FileInfo(outputFilePath).Length);
        var header = new byte[5];
        await using (var stream = File.OpenRead(outputFilePath))
        {
            await stream.ReadExactlyAsync(header);
        }

        Encoding.ASCII.GetString(header).ShouldBe("PGDMP");
    }

    [PgDumpFact]
    public async Task GivenWrongPassword_WhenExporting_ThenThrowsWithPgDumpFailure()
    {
        // Arrange
        var connectionString = new NpgsqlConnectionStringBuilder(database.ConnectionString)
        {
            Password = "wrong-password",
        }.ConnectionString;
        var exporter = CreateExporter(connectionString);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            exporter.ExportAsync(Path.Combine(_outputDirectory, "backup.dump"), CancellationToken.None));

        // Assert
        exception.ShouldBeOfType<InvalidOperationException>().Message.ShouldStartWith("pg_dump failed with exit code");
    }

    public void Dispose()
    {
        if (Directory.Exists(_outputDirectory))
        {
            Directory.Delete(_outputDirectory, recursive: true);
        }
    }

    private static PostgresDumpExporter CreateExporter(string connectionString)
    {
        var clock = Substitute.For<IClock>();
        clock.UtcNow.Returns(DateTimeOffset.UtcNow);

        return new PostgresDumpExporter(
            Options.Create(TestBackupOptions.Valid() with { DatabaseConnectionString = connectionString }),
            new PostgresConnectionInfoParser(),
            clock,
            NullLogger<PostgresDumpExporter>.Instance
        );
    }
}
