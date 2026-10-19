using Devlivery.BackupJob.Models;

namespace Devlivery.BackupJob.Services;

public interface IPostgresDumpExporter
{
    Task<DumpResult> ExportAsync(string outputFilePath, CancellationToken cancellationToken);
}