namespace Devlivery.BackupJob.Models;

public sealed record DumpResult(
    string FilePath,
    long SizeInBytes,
    string DatabaseName,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc
);