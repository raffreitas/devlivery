using Devlivery.BackupJob.Configuration;

namespace Devlivery.BackupJob.Tests.Common;

public static class TestBackupOptions
{
    public static BackupOptions Valid() => new()
    {
        ApplicationName = "devlivery",
        EnvironmentName = "production",
        DatabaseConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=secret;Database=devlivery",
        BucketName = "devlivery-backups",
        BucketPrefix = "postgres",
        R2Endpoint = "https://account.r2.cloudflarestorage.com",
        AccessKeyId = "access-key",
        SecretAccessKey = "secret-key",
    };
}
