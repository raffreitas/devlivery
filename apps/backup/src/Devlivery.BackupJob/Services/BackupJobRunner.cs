using System.Diagnostics;

using Devlivery.BackupJob.Configuration;
using Devlivery.BackupJob.Models;
using Devlivery.BackupJob.Observability;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Devlivery.BackupJob.Services;

public sealed class BackupJobRunner(
    IOptions<BackupOptions> backupOptions,
    IClock clock,
    IBackupObjectKeyFactory objectKeyFactory,
    IPostgresDumpExporter postgresDumpExporter,
    IFileChecksumProvider fileChecksumProvider,
    IBackupStorageClient backupStorageClient,
    IBackupRetentionService backupRetentionService,
    ILogger<BackupJobRunner> logger)
{
    public async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        var options = backupOptions.Value;
        var startedAtUtc = clock.UtcNow;
        var dumpObjectKey = objectKeyFactory.CreateDumpObjectKey(options, startedAtUtc);
        var manifestObjectKey = objectKeyFactory.CreateManifestObjectKey(options, startedAtUtc);
        var tempDirectory = Path.Combine(Path.GetTempPath(), "devlivery-backups", Guid.NewGuid().ToString("N"));
        var dumpFilePath = Path.Combine(tempDirectory, GetFileName(dumpObjectKey));

        Directory.CreateDirectory(tempDirectory);

        using var backupActivity = BackupTelemetry.ActivitySource.StartActivity("backup.run");
        backupActivity?.SetTag("backup.application", options.ApplicationName);
        backupActivity?.SetTag("backup.environment", options.EnvironmentName);
        backupActivity?.SetTag("backup.bucket", options.BucketName);
        backupActivity?.SetTag("backup.retention_days", options.RetentionDays);
        backupActivity?.SetTag("backup.dump_key", dumpObjectKey);
        backupActivity?.SetTag("backup.manifest_key", manifestObjectKey);

        try
        {
            logger.LogInformation(
                "Starting backup job for environment {EnvironmentName} and bucket {BucketName}",
                options.EnvironmentName,
                options.BucketName
            );

            DumpResult dumpResult;
            using (var exportActivity = BackupTelemetry.ActivitySource.StartActivity("backup.export"))
            {
                exportActivity?.SetTag("backup.output_path", dumpFilePath);
                dumpResult = await postgresDumpExporter.ExportAsync(dumpFilePath, cancellationToken);
                exportActivity?.SetTag("db.name", dumpResult.DatabaseName);
                exportActivity?.SetTag("backup.size_bytes", dumpResult.SizeInBytes);
            }

            string sha256Checksum;
            using (var checksumActivity = BackupTelemetry.ActivitySource.StartActivity("backup.checksum"))
            {
                checksumActivity?.SetTag("backup.file_path", dumpResult.FilePath);
                sha256Checksum = await fileChecksumProvider.ComputeSha256Async(dumpResult.FilePath, cancellationToken);
                checksumActivity?.SetTag("backup.checksum_algorithm", "sha256");
            }

            using (var uploadDumpActivity = BackupTelemetry.ActivitySource.StartActivity("backup.upload.dump"))
            {
                uploadDumpActivity?.SetTag("backup.object_key", dumpObjectKey);
                await backupStorageClient.UploadFileAsync(
                    dumpObjectKey,
                    dumpResult.FilePath,
                    "application/octet-stream",
                    cancellationToken
                );
            }

            int deletedObjectsCount;
            using (var retentionActivity = BackupTelemetry.ActivitySource.StartActivity("backup.retention"))
            {
                deletedObjectsCount = await backupRetentionService.ApplyAsync(cancellationToken);
                retentionActivity?.SetTag("backup.deleted_objects_count", deletedObjectsCount);
            }

            var jobCompletedAtUtc = clock.UtcNow;
            var manifest = new BackupManifest(
                Version: 1,
                ApplicationName: options.ApplicationName,
                EnvironmentName: options.EnvironmentName,
                DatabaseName: dumpResult.DatabaseName,
                BucketName: options.BucketName,
                DumpObjectKey: dumpObjectKey,
                StartedAtUtc: dumpResult.StartedAtUtc,
                DumpCompletedAtUtc: dumpResult.CompletedAtUtc,
                JobCompletedAtUtc: jobCompletedAtUtc,
                DurationSeconds: (jobCompletedAtUtc - dumpResult.StartedAtUtc).TotalSeconds,
                SizeInBytes: dumpResult.SizeInBytes,
                Sha256Checksum: sha256Checksum,
                RetentionDays: options.RetentionDays,
                DeletedObjectsCount: deletedObjectsCount
            );

            using (var uploadManifestActivity = BackupTelemetry.ActivitySource.StartActivity("backup.upload.manifest"))
            {
                uploadManifestActivity?.SetTag("backup.object_key", manifestObjectKey);
                await backupStorageClient.UploadJsonAsync(manifestObjectKey, manifest, cancellationToken);
            }

            backupActivity?.SetStatus(ActivityStatusCode.Ok);
            backupActivity?.SetTag("backup.result", "success");
            backupActivity?.SetTag("backup.duration_seconds", manifest.DurationSeconds);
            backupActivity?.SetTag("backup.deleted_objects_count", deletedObjectsCount);

            BackupTelemetry.RecordRun("success", options.EnvironmentName, options.BucketName);
            BackupTelemetry.RecordDuration(manifest.DurationSeconds, options.EnvironmentName, options.BucketName);
            BackupTelemetry.RecordDumpSize(dumpResult.SizeInBytes, options.EnvironmentName, options.BucketName);
            BackupTelemetry.RecordDeletedObjects(deletedObjectsCount, options.EnvironmentName, options.BucketName);

            logger.LogInformation(
                "Backup job finished successfully. DumpKey: {DumpObjectKey}, ManifestKey: {ManifestObjectKey}, DurationSeconds: {DurationSeconds}, SizeBytes: {SizeBytes}",
                dumpObjectKey,
                manifestObjectKey,
                manifest.DurationSeconds,
                dumpResult.SizeInBytes
            );

            return 0;
        }
        catch (Exception exception)
        {
            backupActivity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            backupActivity?.SetTag("backup.result", "failure");
            backupActivity?.AddException(exception);
            BackupTelemetry.RecordRun("failure", options.EnvironmentName, options.BucketName);
            logger.LogError(exception, "Backup job failed.");
            return 1;
        }
        finally
        {
            TryDeleteDirectory(tempDirectory);
        }
    }

    private static string GetFileName(string objectKey) => objectKey.Split('/').Last();

    private static void TryDeleteDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        try
        {
            Directory.Delete(path, recursive: true);
        }
        catch
        {
            // Best effort cleanup for ephemeral artifacts.
        }
    }
}