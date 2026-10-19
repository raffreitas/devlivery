namespace Devlivery.BackupJob.Models;

public sealed record PostgresConnectionInfo(
    string Host,
    int Port,
    string Username,
    string Password,
    string Database,
    string? SslMode
);