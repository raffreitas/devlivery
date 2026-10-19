using Devlivery.BackupJob.Configuration;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Devlivery.BackupJob.Services;

public interface IBackupRetentionService
{
    Task<int> ApplyAsync(CancellationToken cancellationToken);
}

public sealed class BackupRetentionService(
    IBackupStorageClient backupStorageClient,
    IBackupObjectKeyFactory backupObjectKeyFactory,
    IClock clock,
    IOptions<BackupOptions> backupOptions,
    ILogger<BackupRetentionService> logger) : IBackupRetentionService
{
    public async Task<int> ApplyAsync(CancellationToken cancellationToken)
    {
        var options = backupOptions.Value;
        var cutoffUtc = clock.UtcNow.AddDays(-options.RetentionDays);
        var prefix = backupObjectKeyFactory.CreateEnvironmentPrefix(options);
        var storedObjects = await backupStorageClient.ListAsync(prefix, cancellationToken);
        var objectKeysToDelete = storedObjects
            .Where(storageObject => storageObject.LastModifiedUtc < cutoffUtc)
            .Select(storageObject => storageObject.Key)
            .ToArray();

        if (objectKeysToDelete.Length == 0)
        {
            logger.LogInformation(
                "No backup objects eligible for deletion under prefix {Prefix} using cutoff {CutoffUtc}",
                prefix,
                cutoffUtc
            );

            return 0;
        }

        await backupStorageClient.DeleteAsync(objectKeysToDelete, cancellationToken);

        logger.LogInformation(
            "Deleted {Count} backup objects older than {CutoffUtc} under prefix {Prefix}",
            objectKeysToDelete.Length,
            cutoffUtc,
            prefix
        );

        return objectKeysToDelete.Length;
    }
}