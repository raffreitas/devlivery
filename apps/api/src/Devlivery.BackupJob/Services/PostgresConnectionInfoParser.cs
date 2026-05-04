using Devlivery.BackupJob.Models;

using Npgsql;

namespace Devlivery.BackupJob.Services;

public sealed class PostgresConnectionInfoParser : IPostgresConnectionInfoParser
{
    public PostgresConnectionInfo Parse(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Backup:DatabaseConnectionString is required.");
        }

        return IsPostgresUri(connectionString)
            ? ParseUriConnectionString(connectionString)
            : ParseKeyValueConnectionString(connectionString);
    }

    private static bool IsPostgresUri(string connectionString) =>
        connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
        connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase);

    private static PostgresConnectionInfo ParseKeyValueConnectionString(string connectionString)
    {
        NpgsqlConnectionStringBuilder builder;

        try
        {
            builder = new NpgsqlConnectionStringBuilder(connectionString);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                "Backup:DatabaseConnectionString is not a valid PostgreSQL connection string.", exception);
        }

        if (string.IsNullOrWhiteSpace(builder.Host) ||
            string.IsNullOrWhiteSpace(builder.Username) ||
            string.IsNullOrWhiteSpace(builder.Database))
        {
            throw new InvalidOperationException(
                "Backup:DatabaseConnectionString must include host, username and database.");
        }

        return new PostgresConnectionInfo(
            builder.Host,
            builder.Port,
            builder.Username,
            builder.Password ?? string.Empty,
            builder.Database,
            builder.SslMode.ToString().ToLowerInvariant()
        );
    }

    private static PostgresConnectionInfo ParseUriConnectionString(string connectionString)
    {
        Uri uri;

        try
        {
            uri = new Uri(connectionString);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException("Backup:DatabaseConnectionString is not a valid PostgreSQL URI.",
                exception);
        }

        if (string.IsNullOrWhiteSpace(uri.Host))
        {
            throw new InvalidOperationException("Backup:DatabaseConnectionString URI must include a host.");
        }

        var userInfoSegments = uri.UserInfo.Split(':', count: 2);
        var username = userInfoSegments.Length > 0 ? Uri.UnescapeDataString(userInfoSegments[0]) : string.Empty;
        var password = userInfoSegments.Length > 1 ? Uri.UnescapeDataString(userInfoSegments[1]) : string.Empty;
        var database = Uri.UnescapeDataString(uri.AbsolutePath.Trim('/'));
        var sslMode = GetQueryValue(uri.Query, "sslmode");

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(database))
        {
            throw new InvalidOperationException(
                "Backup:DatabaseConnectionString URI must include username and database.");
        }

        return new PostgresConnectionInfo(
            Host: uri.Host,
            Port: uri.Port > 0 ? uri.Port : 5432,
            Username: username,
            Password: password,
            Database: database,
            SslMode: sslMode
        );
    }

    private static string? GetQueryValue(string queryString, string key)
    {
        if (string.IsNullOrWhiteSpace(queryString))
        {
            return null;
        }

        var segments = queryString.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var segment in segments)
        {
            var pair = segment.Split('=', count: 2);

            if (!pair[0].Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return pair.Length == 2 ? Uri.UnescapeDataString(pair[1]) : string.Empty;
        }

        return null;
    }
}