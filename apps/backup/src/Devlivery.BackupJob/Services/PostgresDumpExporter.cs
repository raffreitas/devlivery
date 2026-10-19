using System.Diagnostics;

using Devlivery.BackupJob.Configuration;
using Devlivery.BackupJob.Models;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Devlivery.BackupJob.Services;

public sealed class PostgresDumpExporter(
    IOptions<BackupOptions> backupOptions,
    IPostgresConnectionInfoParser postgresConnectionInfoParser,
    IClock clock,
    ILogger<PostgresDumpExporter> logger) : IPostgresDumpExporter
{
    public async Task<DumpResult> ExportAsync(string outputFilePath, CancellationToken cancellationToken)
    {
        var options = backupOptions.Value;
        var connectionInfo = postgresConnectionInfoParser.Parse(options.DatabaseConnectionString);
        var startedAtUtc = clock.UtcNow;

        Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath)!);

        using var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedCancellationTokenSource.CancelAfter(TimeSpan.FromMinutes(options.DumpTimeoutMinutes));

        var processStartInfo = new ProcessStartInfo
        {
            FileName = options.PgDumpPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        processStartInfo.ArgumentList.Add("--format=custom");
        processStartInfo.ArgumentList.Add($"--compress={options.DumpCompressionLevel}");
        processStartInfo.ArgumentList.Add("--no-owner");
        processStartInfo.ArgumentList.Add("--no-privileges");
        processStartInfo.ArgumentList.Add("--file");
        processStartInfo.ArgumentList.Add(outputFilePath);
        processStartInfo.ArgumentList.Add("--dbname");
        processStartInfo.ArgumentList.Add(connectionInfo.Database);

        processStartInfo.Environment["PGHOST"] = connectionInfo.Host;
        processStartInfo.Environment["PGPORT"] = connectionInfo.Port.ToString();
        processStartInfo.Environment["PGUSER"] = connectionInfo.Username;
        processStartInfo.Environment["PGPASSWORD"] = connectionInfo.Password;

        if (!string.IsNullOrWhiteSpace(connectionInfo.SslMode))
        {
            processStartInfo.Environment["PGSSLMODE"] = connectionInfo.SslMode;
        }

        logger.LogInformation(
            "Starting pg_dump for database {Database} using host {Host}",
            connectionInfo.Database,
            connectionInfo.Host
        );

        using var process = new Process();
        process.StartInfo = processStartInfo;

        process.Start();

        var standardOutputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var standardErrorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        try
        {
            await process.WaitForExitAsync(linkedCancellationTokenSource.Token);
        }
        catch (OperationCanceledException exception)
        {
            TryKill(process);

            throw new TimeoutException(
                $"pg_dump did not finish within {options.DumpTimeoutMinutes} minute(s).",
                exception
            );
        }

        var standardOutput = await standardOutputTask;
        var standardError = await standardErrorTask;

        if (!string.IsNullOrWhiteSpace(standardOutput))
        {
            logger.LogDebug("pg_dump output: {Output}", standardOutput.Trim());
        }

        if (!string.IsNullOrWhiteSpace(standardError))
        {
            logger.LogInformation("pg_dump messages: {Output}", standardError.Trim());
        }

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"pg_dump failed with exit code {process.ExitCode}: {standardError}");
        }

        var fileInfo = new FileInfo(outputFilePath);

        if (!fileInfo.Exists || fileInfo.Length == 0)
        {
            throw new InvalidOperationException(
                "pg_dump finished successfully but the backup artifact was not created.");
        }

        var completedAtUtc = clock.UtcNow;

        logger.LogInformation(
            "pg_dump completed for database {Database} with size {SizeInBytes} bytes",
            connectionInfo.Database,
            fileInfo.Length
        );

        return new DumpResult(outputFilePath, fileInfo.Length, connectionInfo.Database, startedAtUtc, completedAtUtc);
    }

    private static void TryKill(Process process)
    {
        if (process.HasExited)
        {
            return;
        }

        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch
        {
            // Best effort termination.
        }
    }
}