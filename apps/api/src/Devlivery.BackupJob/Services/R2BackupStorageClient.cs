using System.Text.Json;

using Amazon.S3;
using Amazon.S3.Model;

using Devlivery.BackupJob.Configuration;
using Devlivery.BackupJob.Models;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Devlivery.BackupJob.Services;

public sealed class R2BackupStorageClient(
    IAmazonS3 amazonS3,
    IOptions<BackupOptions> backupOptions,
    ILogger<R2BackupStorageClient> logger) : IBackupStorageClient
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true,
    };

    public async Task UploadFileAsync(string objectKey, string filePath, string contentType, CancellationToken cancellationToken)
    {
        var options = backupOptions.Value;
        var request = new PutObjectRequest
        {
            BucketName = options.BucketName,
            Key = objectKey,
            FilePath = filePath,
            ContentType = contentType,
        };

        await amazonS3.PutObjectAsync(request, cancellationToken);

        logger.LogInformation(
            "Uploaded object {ObjectKey} to bucket {BucketName}",
            objectKey,
            options.BucketName
        );
    }

    public async Task UploadJsonAsync<T>(string objectKey, T content, CancellationToken cancellationToken)
    {
        var options = backupOptions.Value;
        var request = new PutObjectRequest
        {
            BucketName = options.BucketName,
            Key = objectKey,
            ContentBody = JsonSerializer.Serialize(content, JsonSerializerOptions),
            ContentType = "application/json; charset=utf-8",
        };

        await amazonS3.PutObjectAsync(request, cancellationToken);

        logger.LogInformation(
            "Uploaded manifest {ObjectKey} to bucket {BucketName}",
            objectKey,
            options.BucketName
        );
    }

    public async Task<IReadOnlyList<StorageObject>> ListAsync(string prefix, CancellationToken cancellationToken)
    {
        var options = backupOptions.Value;
        var objects = new List<StorageObject>();
        string? continuationToken = null;

        do
        {
            var response = await amazonS3.ListObjectsV2Async(
                new ListObjectsV2Request
                {
                    BucketName = options.BucketName,
                    Prefix = prefix,
                    ContinuationToken = continuationToken,
                },
                cancellationToken
            );

            objects.AddRange(response.S3Objects.Select(storageObject =>
                new StorageObject(
                    storageObject.Key,
                    new DateTimeOffset((storageObject.LastModified ?? DateTime.UnixEpoch).ToUniversalTime())
                )));

            continuationToken = response.IsTruncated == true ? response.NextContinuationToken : null;
        }
        while (!string.IsNullOrWhiteSpace(continuationToken));

        return objects;
    }

    public async Task DeleteAsync(IReadOnlyCollection<string> objectKeys, CancellationToken cancellationToken)
    {
        if (objectKeys.Count == 0)
        {
            return;
        }

        var options = backupOptions.Value;

        foreach (var batch in objectKeys.Chunk(1000))
        {
            var request = new DeleteObjectsRequest
            {
                BucketName = options.BucketName,
                Objects = batch.Select(objectKey => new KeyVersion { Key = objectKey }).ToList(),
            };

            await amazonS3.DeleteObjectsAsync(request, cancellationToken);
        }

        logger.LogInformation(
            "Deleted {Count} objects from bucket {BucketName}",
            objectKeys.Count,
            options.BucketName
        );
    }
}