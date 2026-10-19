using Devlivery.BackupJob.Models;

namespace Devlivery.BackupJob.Services;

public interface IBackupStorageClient
{
    Task UploadFileAsync(string objectKey, string filePath, string contentType, CancellationToken cancellationToken);

    Task UploadJsonAsync<T>(string objectKey, T content, CancellationToken cancellationToken);

    Task<IReadOnlyList<StorageObject>> ListAsync(string prefix, CancellationToken cancellationToken);

    Task DeleteAsync(IReadOnlyCollection<string> objectKeys, CancellationToken cancellationToken);
}