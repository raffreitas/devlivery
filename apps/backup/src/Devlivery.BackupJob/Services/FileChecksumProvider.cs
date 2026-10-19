using System.Security.Cryptography;

namespace Devlivery.BackupJob.Services;

public interface IFileChecksumProvider
{
    Task<string> ComputeSha256Async(string filePath, CancellationToken cancellationToken);
}

public sealed class FileChecksumProvider : IFileChecksumProvider
{
    public async Task<string> ComputeSha256Async(string filePath, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(filePath);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}