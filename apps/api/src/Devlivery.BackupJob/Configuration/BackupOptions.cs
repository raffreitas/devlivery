using System.ComponentModel.DataAnnotations;

namespace Devlivery.BackupJob.Configuration;

public sealed class BackupOptions
{
    public const string SectionName = "Backup";

    [Required] public string ApplicationName { get; set; } = "devlivery";

    [Required] public string EnvironmentName { get; set; } = string.Empty;

    [Required] public string DatabaseConnectionString { get; init; } = string.Empty;

    [Required] public string BucketName { get; init; } = string.Empty;

    public string BucketPrefix { get; init; } = "postgres";

    [Required] public string R2Endpoint { get; init; } = string.Empty;

    [Required] public string AccessKeyId { get; init; } = string.Empty;

    [Required] public string SecretAccessKey { get; init; } = string.Empty;

    public string PgDumpPath { get; init; } = "pg_dump";

    public int RetentionDays { get; init; } = 7;

    public int DumpCompressionLevel { get; init; } = 9;

    public int DumpTimeoutMinutes { get; init; } = 30;

    public bool ForcePathStyle { get; init; } = true;
}