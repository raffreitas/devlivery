using System.Text;

using Devlivery.BackupJob.Configuration;

namespace Devlivery.BackupJob.Services;

public interface IBackupObjectKeyFactory
{
    string CreateDumpObjectKey(BackupOptions options, DateTimeOffset timestampUtc);

    string CreateManifestObjectKey(BackupOptions options, DateTimeOffset timestampUtc);

    string CreateEnvironmentPrefix(BackupOptions options);
}

public sealed class BackupObjectKeyFactory : IBackupObjectKeyFactory
{
    public string CreateDumpObjectKey(BackupOptions options, DateTimeOffset timestampUtc) =>
        BuildObjectKey(options, timestampUtc, ".dump");

    public string CreateManifestObjectKey(BackupOptions options, DateTimeOffset timestampUtc) =>
        BuildObjectKey(options, timestampUtc, ".manifest.json");

    public string CreateEnvironmentPrefix(BackupOptions options)
    {
        var segments = new[]
        {
            NormalizeSegment(options.BucketPrefix),
            NormalizeSegment(options.EnvironmentName),
        };

        return string.Join('/', segments.Where(segment => !string.IsNullOrWhiteSpace(segment)));
    }

    private string BuildObjectKey(BackupOptions options, DateTimeOffset timestampUtc, string extension)
    {
        var normalizedTimestamp = timestampUtc.ToUniversalTime();
        var prefix = CreateEnvironmentPrefix(options);
        var fileName = $"{NormalizeSegment(options.ApplicationName)}-{NormalizeSegment(options.EnvironmentName)}-{normalizedTimestamp:yyyyMMdd'T'HHmmss'Z'}{extension}";

        var segments = new[]
        {
            prefix,
            normalizedTimestamp.ToString("yyyy"),
            normalizedTimestamp.ToString("MM"),
            normalizedTimestamp.ToString("dd"),
            fileName,
        };

        return string.Join('/', segments.Where(segment => !string.IsNullOrWhiteSpace(segment)));
    }

    private static string NormalizeSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);

        foreach (var character in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character) || character is '-' or '_')
            {
                builder.Append(character);
                continue;
            }

            if (builder.Length == 0 || builder[^1] == '-')
            {
                continue;
            }

            builder.Append('-');
        }

        return builder.ToString().Trim('-');
    }
}