using Devlivery.Common.Time;

using Microsoft.Extensions.Time.Testing;

using Shouldly;

namespace Devlivery.Tests.Common.Time;

[Trait("Category", "Unit Tests")]
public sealed class BrazilTimeTests
{
    [Fact]
    public void GetBrazilToday_Should_Return_Previous_Day_When_Utc_Has_Already_Turned()
    {
        // Arrange: 02:00 UTC on Oct 15 is still 23:00 on Oct 14 in São Paulo (UTC-3)
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 10, 15, 2, 0, 0, TimeSpan.Zero));

        // Act
        var today = timeProvider.GetBrazilToday();

        // Assert
        today.ShouldBe(new DateOnly(2026, 10, 14));
    }

    [Fact]
    public void GetBrazilNow_Should_Convert_Utc_To_Sao_Paulo_Time()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 10, 15, 15, 30, 0, TimeSpan.Zero));

        // Act
        var now = timeProvider.GetBrazilNow();

        // Assert
        now.ShouldBe(new DateTime(2026, 10, 15, 12, 30, 0));
    }

    [Fact]
    public void Brazilian_Day_Boundaries_Should_Be_Converted_To_Utc()
    {
        // Arrange
        var day = new DateOnly(2026, 10, 14);

        // Act & Assert: São Paulo is UTC-3, so the local day runs from 03:00 UTC to 03:00 UTC of the next day
        day.ToBrazilStartOfDayUtc().ShouldBe(new DateTime(2026, 10, 14, 3, 0, 0, DateTimeKind.Utc));
        day.ToBrazilEndOfDayExclusiveUtc().ShouldBe(new DateTime(2026, 10, 15, 3, 0, 0, DateTimeKind.Utc));
    }

    [Theory]
    [InlineData(DateTimeKind.Utc)]
    [InlineData(DateTimeKind.Unspecified)]
    public void ToBrazilTime_Should_Treat_Value_As_Utc(DateTimeKind kind)
    {
        // Arrange
        var utc = new DateTime(2026, 10, 15, 1, 0, 0, kind);

        // Act
        var local = utc.ToBrazilTime();

        // Assert
        local.ShouldBe(new DateTime(2026, 10, 14, 22, 0, 0));
    }
}