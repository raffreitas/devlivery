using Devlivery.BackupJob.Services;

using Shouldly;

namespace Devlivery.BackupJob.Tests.Services;

[Trait("Category", "Unit Tests")]
public sealed class FileChecksumProviderTests : IDisposable
{
    private readonly string _filePath = Path.GetTempFileName();

    [Fact]
    public async Task GivenFile_WhenComputingSha256_ThenReturnsLowercaseHexDigest()
    {
        // Arrange
        await File.WriteAllTextAsync(_filePath, "abc");
        var provider = new FileChecksumProvider();

        // Act
        var checksum = await provider.ComputeSha256Async(_filePath, CancellationToken.None);

        // Assert
        checksum.ShouldBe("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad");
    }

    public void Dispose() => File.Delete(_filePath);
}
