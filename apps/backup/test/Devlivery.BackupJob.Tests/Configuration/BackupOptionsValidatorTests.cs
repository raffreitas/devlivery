using Devlivery.BackupJob.Configuration;
using Devlivery.BackupJob.Tests.Common;

using Shouldly;

namespace Devlivery.BackupJob.Tests.Configuration;

[Trait("Category", "Unit Tests")]
public sealed class BackupOptionsValidatorTests
{
    private readonly BackupOptionsValidator _validator = new();

    public static TheoryData<BackupOptions, string> InvalidOptions => new()
    {
        { TestBackupOptions.Valid() with { RetentionDays = 0 }, "Backup:RetentionDays must be greater than zero." },
        { TestBackupOptions.Valid() with { DumpCompressionLevel = -1 }, "Backup:DumpCompressionLevel must be between 0 and 9." },
        { TestBackupOptions.Valid() with { DumpCompressionLevel = 10 }, "Backup:DumpCompressionLevel must be between 0 and 9." },
        { TestBackupOptions.Valid() with { DumpTimeoutMinutes = 0 }, "Backup:DumpTimeoutMinutes must be greater than zero." },
        { TestBackupOptions.Valid() with { PgDumpPath = " " }, "Backup:PgDumpPath is required." },
        { TestBackupOptions.Valid() with { R2Endpoint = "not-a-url" }, "Backup:R2Endpoint must be a valid absolute HTTP or HTTPS URL." },
        { TestBackupOptions.Valid() with { R2Endpoint = "ftp://account.r2.cloudflarestorage.com" }, "Backup:R2Endpoint must be a valid absolute HTTP or HTTPS URL." },
    };

    [Fact]
    public void GivenValidOptions_WhenValidating_ThenSucceeds()
    {
        // Arrange
        var options = TestBackupOptions.Valid();

        // Act
        var result = _validator.Validate(name: null, options);

        // Assert
        result.Succeeded.ShouldBeTrue();
    }

    [Theory]
    [MemberData(nameof(InvalidOptions))]
    public void GivenInvalidOptions_WhenValidating_ThenFailsWithMessage(BackupOptions options, string expectedFailure)
    {
        // Act
        var result = _validator.Validate(name: null, options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.Failures.ShouldNotBeNull().ShouldContain(expectedFailure);
    }
}
