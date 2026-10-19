using System.Text.Json;

using Amazon.S3;
using Amazon.S3.Model;

using Devlivery.BackupJob.Services;
using Devlivery.BackupJob.Tests.Common;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NSubstitute;

using Shouldly;

namespace Devlivery.BackupJob.Tests.Services;

[Trait("Category", "Unit Tests")]
public sealed class R2BackupStorageClientTests
{
    private const string BucketName = "devlivery-backups";

    private readonly IAmazonS3 _amazonS3 = Substitute.For<IAmazonS3>();
    private readonly R2BackupStorageClient _client;

    public R2BackupStorageClientTests()
    {
        _client = new R2BackupStorageClient(
            _amazonS3,
            Options.Create(TestBackupOptions.Valid()),
            NullLogger<R2BackupStorageClient>.Instance
        );
    }

    [Fact]
    public async Task GivenDumpFile_WhenUploading_ThenSendsUnsignedPayloadWithoutChunkEncoding()
    {
        // Arrange
        PutObjectRequest? request = null;
        await _amazonS3.PutObjectAsync(Arg.Do<PutObjectRequest>(sent => request = sent), Arg.Any<CancellationToken>());

        // Act
        await _client.UploadFileAsync("backup.dump", "/tmp/backup.dump", "application/octet-stream", CancellationToken.None);

        // Assert
        request.ShouldNotBeNull();
        request.BucketName.ShouldBe(BucketName);
        request.Key.ShouldBe("backup.dump");
        request.FilePath.ShouldBe("/tmp/backup.dump");
        request.ContentType.ShouldBe("application/octet-stream");
        request.UseChunkEncoding.ShouldBe(false);
        request.DisablePayloadSigning.ShouldBe(true);
    }

    [Fact]
    public async Task GivenContent_WhenUploadingJson_ThenSendsSerializedBodyWithoutChunkEncoding()
    {
        // Arrange
        PutObjectRequest? request = null;
        await _amazonS3.PutObjectAsync(Arg.Do<PutObjectRequest>(sent => request = sent), Arg.Any<CancellationToken>());

        // Act
        await _client.UploadJsonAsync("backup.manifest.json", new { Version = 1 }, CancellationToken.None);

        // Assert
        request.ShouldNotBeNull();
        request.BucketName.ShouldBe(BucketName);
        request.Key.ShouldBe("backup.manifest.json");
        request.ContentType.ShouldBe("application/json; charset=utf-8");
        JsonDocument.Parse(request.ContentBody).RootElement.GetProperty("Version").GetInt32().ShouldBe(1);
        request.UseChunkEncoding.ShouldBe(false);
        request.DisablePayloadSigning.ShouldBe(true);
    }

    [Fact]
    public async Task GivenTruncatedListing_WhenListing_ThenFollowsContinuationTokens()
    {
        // Arrange
        var firstModifiedAt = new DateTime(2026, 10, 18, 3, 0, 0, DateTimeKind.Utc);
        var secondModifiedAt = new DateTime(2026, 10, 19, 3, 0, 0, DateTimeKind.Utc);

        _amazonS3.ListObjectsV2Async(
                Arg.Is<ListObjectsV2Request>(request =>
                    request.BucketName == BucketName && request.Prefix == "postgres" && request.ContinuationToken == null),
                Arg.Any<CancellationToken>())
            .Returns(new ListObjectsV2Response
            {
                S3Objects = [new S3Object { Key = "first.dump", LastModified = firstModifiedAt }],
                IsTruncated = true,
                NextContinuationToken = "page-2",
            });
        _amazonS3.ListObjectsV2Async(
                Arg.Is<ListObjectsV2Request>(request => request.ContinuationToken == "page-2"),
                Arg.Any<CancellationToken>())
            .Returns(new ListObjectsV2Response
            {
                S3Objects = [new S3Object { Key = "second.dump", LastModified = secondModifiedAt }],
                IsTruncated = false,
            });

        // Act
        var objects = await _client.ListAsync("postgres", CancellationToken.None);

        // Assert
        objects.Select(storageObject => storageObject.Key).ShouldBe(["first.dump", "second.dump"]);
        objects.Select(storageObject => storageObject.LastModifiedUtc).ShouldBe([
            new DateTimeOffset(firstModifiedAt),
            new DateTimeOffset(secondModifiedAt),
        ]);
    }

    [Fact]
    public async Task GivenMoreKeysThanS3Limit_WhenDeleting_ThenSendsBatchesOfOneThousand()
    {
        // Arrange
        var keys = Enumerable.Range(0, 2500).Select(index => $"backup-{index}.dump").ToArray();
        var requests = new List<DeleteObjectsRequest>();
        await _amazonS3.DeleteObjectsAsync(Arg.Do<DeleteObjectsRequest>(requests.Add), Arg.Any<CancellationToken>());

        // Act
        await _client.DeleteAsync(keys, CancellationToken.None);

        // Assert
        requests.Select(request => request.Objects.Count).ShouldBe([1000, 1000, 500]);
        requests.ShouldAllBe(request => request.BucketName == BucketName);
        requests.SelectMany(request => request.Objects).Select(keyVersion => keyVersion.Key).ShouldBe(keys);
    }

    [Fact]
    public async Task GivenNoKeys_WhenDeleting_ThenDoesNotCallStorage()
    {
        // Act
        await _client.DeleteAsync([], CancellationToken.None);

        // Assert
        _amazonS3.ReceivedCalls().ShouldBeEmpty();
    }
}
