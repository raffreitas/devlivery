using Devlivery.Common.Time;

using Microsoft.Extensions.Time.Testing;

using Shouldly;

namespace Devlivery.Tests.Common.Time;

[Trait("Category", "Unit Tests")]
public sealed class BrazilTimeTests
{
    [Fact]
    public void GivenUtcAlreadyOnNextDay_WhenGettingBrazilToday_ThenReturnsPreviousDay()
    {
        // Arrange: 02:00 UTC on Oct 15 is still 23:00 on Oct 14 in São Paulo (UTC-3)
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 10, 15, 2, 0, 0, TimeSpan.Zero));

        // Act
        var today = timeProvider.GetBrazilToday();

        // Assert
        today.ShouldBe(new DateOnly(2026, 10, 14));
    }

    [Fact]
    public void GivenUtcInstant_WhenGettingBrazilNow_ThenReturnsSaoPauloTime()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 10, 15, 15, 30, 0, TimeSpan.Zero));

        // Act
        var now = timeProvider.GetBrazilNow();

        // Assert
        now.ShouldBe(new DateTime(2026, 10, 15, 12, 30, 0));
    }

    // São Paulo is UTC-3, so a local day runs from 03:00 UTC to 03:00 UTC of the next day
    [Fact]
    public void GivenBrazilianDay_WhenGettingStartOfDayUtc_ThenReturnsThreeAmUtcOfSameDay()
    {
        // Arrange
        var day = new DateOnly(2026, 10, 14);

        // Act
        var start = day.ToBrazilStartOfDayUtc();

        // Assert
        start.ShouldBe(new DateTime(2026, 10, 14, 3, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void GivenBrazilianDay_WhenGettingExclusiveEndOfDayUtc_ThenReturnsThreeAmUtcOfNextDay()
    {
        // Arrange
        var day = new DateOnly(2026, 10, 14);

        // Act
        var end = day.ToBrazilEndOfDayExclusiveUtc();

        // Assert
        end.ShouldBe(new DateTime(2026, 10, 15, 3, 0, 0, DateTimeKind.Utc));
    }

    [Theory]
    [InlineData(DateTimeKind.Utc)]
    [InlineData(DateTimeKind.Unspecified)]
    public void GivenUtcOrUnspecifiedKind_WhenConvertingToBrazilTime_ThenTreatsValueAsUtc(DateTimeKind kind)
    {
        // Arrange
        var utc = new DateTime(2026, 10, 15, 1, 0, 0, kind);

        // Act
        var local = utc.ToBrazilTime();

        // Assert
        local.ShouldBe(new DateTime(2026, 10, 14, 22, 0, 0));
    }
}