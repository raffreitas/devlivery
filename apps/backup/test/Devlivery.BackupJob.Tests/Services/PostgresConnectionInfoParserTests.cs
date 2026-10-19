using Devlivery.BackupJob.Services;

using Shouldly;

namespace Devlivery.BackupJob.Tests.Services;

[Trait("Category", "Unit Tests")]
public sealed class PostgresConnectionInfoParserTests
{
    private readonly PostgresConnectionInfoParser _parser = new();

    [Fact]
    public void GivenKeyValueConnectionString_WhenParsing_ThenReadsAllParts()
    {
        // Arrange
        const string connectionString =
            "Host=db.internal;Port=6543;Username=backup;Password=s3cr3t;Database=devlivery;SSL Mode=Require";

        // Act
        var info = _parser.Parse(connectionString);

        // Assert
        info.Host.ShouldBe("db.internal");
        info.Port.ShouldBe(6543);
        info.Username.ShouldBe("backup");
        info.Password.ShouldBe("s3cr3t");
        info.Database.ShouldBe("devlivery");
        info.SslMode.ShouldBe("require");
    }

    [Fact]
    public void GivenUriWithEscapedCredentials_WhenParsing_ThenUnescapesThem()
    {
        // Arrange
        const string connectionString = "postgresql://backup:p%40ss%3Aword@db.internal:6543/devlivery?sslmode=require";

        // Act
        var info = _parser.Parse(connectionString);

        // Assert
        info.Host.ShouldBe("db.internal");
        info.Port.ShouldBe(6543);
        info.Username.ShouldBe("backup");
        info.Password.ShouldBe("p@ss:word");
        info.Database.ShouldBe("devlivery");
        info.SslMode.ShouldBe("require");
    }

    [Fact]
    public void GivenUriWithoutPortAndSslMode_WhenParsing_ThenUsesDefaultPortAndNoSslMode()
    {
        // Arrange
        const string connectionString = "postgres://backup:secret@db.internal/devlivery";

        // Act
        var info = _parser.Parse(connectionString);

        // Assert
        info.Port.ShouldBe(5432);
        info.SslMode.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Host=localhost;Database=devlivery")]
    [InlineData("Host=localhost;Username=backup;Unknown Option=1")]
    [InlineData("postgresql://db.internal/devlivery")]
    [InlineData("postgresql://backup:secret@db.internal")]
    public void GivenIncompleteConnectionString_WhenParsing_ThenThrowsInvalidOperationException(
        string connectionString)
    {
        // Act
        var exception = Record.Exception(() => _parser.Parse(connectionString));

        // Assert
        exception.ShouldBeOfType<InvalidOperationException>();
    }
}
