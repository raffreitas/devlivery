namespace Devlivery.BackupJob.Tests.Common;

public sealed class PgDumpFactAttribute : FactAttribute
{
    public PgDumpFactAttribute()
    {
        if (!IsPgDumpAvailable())
        {
            Skip = "pg_dump not found on PATH. Install the PostgreSQL 17 client to run this test.";
        }
    }

    private static bool IsPgDumpAvailable()
    {
        var fileName = OperatingSystem.IsWindows() ? "pg_dump.exe" : "pg_dump";
        var directories = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

        return directories.Any(directory => File.Exists(Path.Combine(directory, fileName)));
    }
}
