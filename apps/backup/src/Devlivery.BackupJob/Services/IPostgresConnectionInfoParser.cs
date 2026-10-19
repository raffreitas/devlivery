using Devlivery.BackupJob.Models;

namespace Devlivery.BackupJob.Services;

public interface IPostgresConnectionInfoParser
{
    PostgresConnectionInfo Parse(string connectionString);
}