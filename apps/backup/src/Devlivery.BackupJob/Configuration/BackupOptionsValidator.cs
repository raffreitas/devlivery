using Microsoft.Extensions.Options;

namespace Devlivery.BackupJob.Configuration;

public sealed class BackupOptionsValidator : IValidateOptions<BackupOptions>
{
    public ValidateOptionsResult Validate(string? name, BackupOptions options)
    {
        var failures = new List<string>();

        if (options.RetentionDays < 1)
        {
            failures.Add("Backup:RetentionDays must be greater than zero.");
        }

        if (options.DumpCompressionLevel is < 0 or > 9)
        {
            failures.Add("Backup:DumpCompressionLevel must be between 0 and 9.");
        }

        if (options.DumpTimeoutMinutes < 1)
        {
            failures.Add("Backup:DumpTimeoutMinutes must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(options.PgDumpPath))
        {
            failures.Add("Backup:PgDumpPath is required.");
        }

        if (!Uri.TryCreate(options.R2Endpoint, UriKind.Absolute, out var endpoint) ||
            (endpoint.Scheme != Uri.UriSchemeHttp && endpoint.Scheme != Uri.UriSchemeHttps))
        {
            failures.Add("Backup:R2Endpoint must be a valid absolute HTTP or HTTPS URL.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}