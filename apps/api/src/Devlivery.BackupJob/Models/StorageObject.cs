namespace Devlivery.BackupJob.Models;

public sealed record StorageObject(string Key, DateTimeOffset LastModifiedUtc);