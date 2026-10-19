using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Devlivery.BackupJob.Observability;

public static class BackupTelemetry
{
    public const string ActivitySourceName = "Devlivery.BackupJob";
    public const string MeterName = "Devlivery.BackupJob";

    private static readonly Meter Meter = new(MeterName);

    private static readonly Counter<long> BackupRuns = Meter.CreateCounter<long>(
        "backup_runs_total",
        unit: "runs",
        description: "Total number of backup job executions grouped by status."
    );

    private static readonly Histogram<double> BackupDuration = Meter.CreateHistogram<double>(
        "backup_duration_seconds",
        unit: "s",
        description: "Total duration of a backup job execution in seconds."
    );

    private static readonly Histogram<long> BackupDumpSize = Meter.CreateHistogram<long>(
        "backup_dump_size_bytes",
        unit: "By",
        description: "Size of the generated PostgreSQL dump in bytes."
    );

    private static readonly Counter<long> BackupDeletedObjects = Meter.CreateCounter<long>(
        "backup_retention_deleted_objects_total",
        unit: "objects",
        description: "Total number of objects removed by backup retention."
    );

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    public static void RecordRun(string status, string environment, string bucketName)
    {
        BackupRuns.Add(1, CreateTags(status, environment, bucketName));
    }

    public static void RecordDuration(double durationSeconds, string environment, string bucketName)
    {
        BackupDuration.Record(durationSeconds, CreateTags(environment, bucketName));
    }

    public static void RecordDumpSize(long sizeInBytes, string environment, string bucketName)
    {
        BackupDumpSize.Record(sizeInBytes, CreateTags(environment, bucketName));
    }

    public static void RecordDeletedObjects(int count, string environment, string bucketName)
    {
        if (count <= 0)
        {
            return;
        }

        BackupDeletedObjects.Add(count, CreateTags(environment, bucketName));
    }

    private static TagList CreateTags(string environment, string bucketName)
    {
        return new TagList
        {
            { "backup.environment", environment },
            { "backup.bucket", bucketName },
        };
    }

    private static TagList CreateTags(string status, string environment, string bucketName)
    {
        var tags = CreateTags(environment, bucketName);
        tags.Add("backup.status", status);
        return tags;
    }
}