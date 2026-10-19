using Devlivery.BackupJob.Models;
using Devlivery.BackupJob.Services;
using Devlivery.BackupJob.Tests.Common;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NSubstitute;

using Shouldly;

namespace Devlivery.BackupJob.Tests.Services;

[Trait("Category", "Unit Tests")]
public sealed class BackupRetentionServiceTests
{
    private const string EnvironmentPrefix = "postgres/production";
    private static readonly DateTimeOffset Now = new(2026, 10, 19, 3, 0, 0, TimeSpan.Zero);

    private readonly IBackupStorageClient _storageClient = Substitute.For<IBackupStorageClient>();
    private readonly BackupRetentionService _service;

    public BackupRetentionServiceTests()
    {
        var clock = Substitute.For<IClock>();
        clock.UtcNow.Returns(Now);

        _service = new BackupRetentionService(
            _storageClient,
            new BackupObjectKeyFactory(),
            clock,
            Options.Create(TestBackupOptions.Valid() with { RetentionDays = 7 }),
            NullLogger<BackupRetentionService>.Instance
        );
    }

    [Fact]
    public async Task GivenObjectsOlderThanRetention_WhenApplying_ThenDeletesOnlyExpiredObjects()
    {
        // Arrange
        _storageClient.ListAsync(EnvironmentPrefix, Arg.Any<CancellationToken>()).Returns([
            new StorageObject("expired.dump", Now.AddDays(-8)),
            new StorageObject("at-cutoff.dump", Now.AddDays(-7)),
            new StorageObject("recent.dump", Now.AddDays(-1)),
        ]);

        // Act
        var deletedCount = await _service.ApplyAsync(CancellationToken.None);

        // Assert
        deletedCount.ShouldBe(1);
        await _storageClient.Received(1).DeleteAsync(
            Arg.Is<IReadOnlyCollection<string>>(keys => keys.SequenceEqual(new[] { "expired.dump" })),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GivenNoExpiredObjects_WhenApplying_ThenDeletesNothing()
    {
        // Arrange
        _storageClient.ListAsync(EnvironmentPrefix, Arg.Any<CancellationToken>()).Returns([
            new StorageObject("recent.dump", Now.AddDays(-1)),
        ]);

        // Act
        var deletedCount = await _service.ApplyAsync(CancellationToken.None);

        // Assert
        deletedCount.ShouldBe(0);
        await _storageClient.DidNotReceive().DeleteAsync(
            Arg.Any<IReadOnlyCollection<string>>(),
            Arg.Any<CancellationToken>()
        );
    }
}
