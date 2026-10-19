using Devlivery.BackupJob.Services;
using Devlivery.BackupJob.Tests.Common;

using Shouldly;

namespace Devlivery.BackupJob.Tests.Services;

[Trait("Category", "Unit Tests")]
public sealed class BackupObjectKeyFactoryTests
{
    private static readonly DateTimeOffset Timestamp = new(2026, 10, 19, 3, 0, 5, TimeSpan.Zero);

    private readonly BackupObjectKeyFactory _factory = new();

    [Fact]
    public void GivenOptions_WhenCreatingDumpObjectKey_ThenGroupsByEnvironmentAndDate()
    {
        // Arrange
        var options = TestBackupOptions.Valid();

        // Act
        var objectKey = _factory.CreateDumpObjectKey(options, Timestamp);

        // Assert
        objectKey.ShouldBe("postgres/production/2026/10/19/devlivery-production-20261019T030005Z.dump");
    }

    [Fact]
    public void GivenOptions_WhenCreatingManifestObjectKey_ThenSharesDumpPathWithManifestExtension()
    {
        // Arrange
        var options = TestBackupOptions.Valid();

        // Act
        var objectKey = _factory.CreateManifestObjectKey(options, Timestamp);

        // Assert
        objectKey.ShouldBe("postgres/production/2026/10/19/devlivery-production-20261019T030005Z.manifest.json");
    }

    [Fact]
    public void GivenNonUtcTimestamp_WhenCreatingDumpObjectKey_ThenUsesUtcDate()
    {
        // Arrange
        var options = TestBackupOptions.Valid();
        var saoPauloEvening = new DateTimeOffset(2026, 10, 18, 22, 0, 0, TimeSpan.FromHours(-3));

        // Act
        var objectKey = _factory.CreateDumpObjectKey(options, saoPauloEvening);

        // Assert
        objectKey.ShouldBe("postgres/production/2026/10/19/devlivery-production-20261019T010000Z.dump");
    }

    [Fact]
    public void GivenUnsafeNames_WhenCreatingDumpObjectKey_ThenNormalizesSegments()
    {
        // Arrange
        var options = TestBackupOptions.Valid() with
        {
            ApplicationName = " Dev Livery! ",
            EnvironmentName = "Staging EU",
            BucketPrefix = "Backups/PG",
        };

        // Act
        var objectKey = _factory.CreateDumpObjectKey(options, Timestamp);

        // Assert
        objectKey.ShouldBe("backups-pg/staging-eu/2026/10/19/dev-livery-staging-eu-20261019T030005Z.dump");
    }

    [Fact]
    public void GivenEmptyBucketPrefix_WhenCreatingEnvironmentPrefix_ThenUsesOnlyEnvironment()
    {
        // Arrange
        var options = TestBackupOptions.Valid() with { BucketPrefix = "" };

        // Act
        var prefix = _factory.CreateEnvironmentPrefix(options);

        // Assert
        prefix.ShouldBe("production");
    }
}
