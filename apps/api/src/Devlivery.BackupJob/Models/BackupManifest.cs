namespace Devlivery.BackupJob.Models;

public sealed record BackupManifest(
    int Version,
    string ApplicationName,
    string EnvironmentName,
    string DatabaseName,
    string BucketName,
    string DumpObjectKey,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset DumpCompletedAtUtc,
    DateTimeOffset JobCompletedAtUtc,
    double DurationSeconds,
    long SizeInBytes,
    string Sha256Checksum,
    int RetentionDays,
    int DeletedObjectsCount
);