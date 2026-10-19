using Devlivery.BackupJob.Models;
using Devlivery.BackupJob.Services;
using Devlivery.BackupJob.Tests.Common;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using Shouldly;

namespace Devlivery.BackupJob.Tests.Services;

[Trait("Category", "Unit Tests")]
public sealed class BackupJobRunnerTests
{
    private const string DumpObjectKey = "postgres/production/2026/10/19/devlivery-production-20261019T030000Z.dump";
    private const string ManifestObjectKey =
        "postgres/production/2026/10/19/devlivery-production-20261019T030000Z.manifest.json";

    private static readonly DateTimeOffset StartedAt = new(2026, 10, 19, 3, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset DumpCompletedAt = StartedAt.AddSeconds(60);
    private static readonly DateTimeOffset JobCompletedAt = StartedAt.AddSeconds(90);

    private readonly IPostgresDumpExporter _dumpExporter = Substitute.For<IPostgresDumpExporter>();
    private readonly IFileChecksumProvider _checksumProvider = Substitute.For<IFileChecksumProvider>();
    private readonly IBackupStorageClient _storageClient = Substitute.For<IBackupStorageClient>();
    private readonly IBackupRetentionService _retentionService = Substitute.For<IBackupRetentionService>();
    private readonly BackupJobRunner _runner;

    private string? _dumpFilePath;

    public BackupJobRunnerTests()
    {
        var clock = Substitute.For<IClock>();
        clock.UtcNow.Returns(StartedAt, JobCompletedAt);

        _dumpExporter.ExportAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            _dumpFilePath = call.Arg<string>();
            File.WriteAllText(_dumpFilePath, "dump");
            return new DumpResult(_dumpFilePath, 42, "devlivery", StartedAt, DumpCompletedAt);
        });
        _checksumProvider.ComputeSha256Async(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns("abc123");
        _retentionService.ApplyAsync(Arg.Any<CancellationToken>()).Returns(2);

        _runner = new BackupJobRunner(
            Options.Create(TestBackupOptions.Valid()),
            clock,
            new BackupObjectKeyFactory(),
            _dumpExporter,
            _checksumProvider,
            _storageClient,
            _retentionService,
            NullLogger<BackupJobRunner>.Instance
        );
    }

    [Fact]
    public async Task GivenSuccessfulSteps_WhenRunning_ThenReturnsZeroAndUploadsDump()
    {
        // Act
        var exitCode = await _runner.RunAsync(CancellationToken.None);

        // Assert
        exitCode.ShouldBe(0);
        await _storageClient.Received(1).UploadFileAsync(
            DumpObjectKey,
            _dumpFilePath!,
            "application/octet-stream",
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GivenSuccessfulSteps_WhenRunning_ThenUploadsManifestDescribingDump()
    {
        // Arrange
        BackupManifest? manifest = null;
        await _storageClient.UploadJsonAsync(
            ManifestObjectKey,
            Arg.Do<BackupManifest>(uploaded => manifest = uploaded),
            Arg.Any<CancellationToken>()
        );

        // Act
        await _runner.RunAsync(CancellationToken.None);

        // Assert
        manifest.ShouldNotBeNull();
        manifest.DumpObjectKey.ShouldBe(DumpObjectKey);
        manifest.DatabaseName.ShouldBe("devlivery");
        manifest.BucketName.ShouldBe("devlivery-backups");
        manifest.SizeInBytes.ShouldBe(42);
        manifest.Sha256Checksum.ShouldBe("abc123");
        manifest.StartedAtUtc.ShouldBe(StartedAt);
        manifest.DumpCompletedAtUtc.ShouldBe(DumpCompletedAt);
        manifest.JobCompletedAtUtc.ShouldBe(JobCompletedAt);
        manifest.DurationSeconds.ShouldBe(90);
        manifest.RetentionDays.ShouldBe(7);
        manifest.DeletedObjectsCount.ShouldBe(2);
    }

    [Fact]
    public async Task GivenSuccessfulRun_WhenFinished_ThenRemovesTemporaryDumpDirectory()
    {
        // Act
        await _runner.RunAsync(CancellationToken.None);

        // Assert
        Directory.Exists(Path.GetDirectoryName(_dumpFilePath)).ShouldBeFalse();
    }

    [Fact]
    public async Task GivenDumpFailure_WhenRunning_ThenReturnsOneWithoutTouchingStorage()
    {
        // Arrange
        _dumpExporter.ExportAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("pg_dump failed"));

        // Act
        var exitCode = await _runner.RunAsync(CancellationToken.None);

        // Assert
        exitCode.ShouldBe(1);
        _storageClient.ReceivedCalls().ShouldBeEmpty();
        await _retentionService.DidNotReceive().ApplyAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenDumpUploadFailure_WhenRunning_ThenReturnsOneWithoutApplyingRetention()
    {
        // Arrange
        _storageClient.UploadFileAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("R2 unavailable"));

        // Act
        var exitCode = await _runner.RunAsync(CancellationToken.None);

        // Assert
        exitCode.ShouldBe(1);
        await _retentionService.DidNotReceive().ApplyAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenDumpUploadFailure_WhenFinished_ThenRemovesTemporaryDumpDirectory()
    {
        // Arrange
        _storageClient.UploadFileAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("R2 unavailable"));

        // Act
        await _runner.RunAsync(CancellationToken.None);

        // Assert
        Directory.Exists(Path.GetDirectoryName(_dumpFilePath)).ShouldBeFalse();
    }
}
