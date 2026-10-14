using Devlivery.Common.Extensions;

using Shouldly;

namespace Devlivery.Tests.Common.Extensions;

[Trait("Category", "Unit Tests")]
public sealed class QueryableExtensionsTests
{
    private sealed record Row(string Name, DateTime CreatedAtUtc);

    // São Paulo is UTC-3: Oct 14 runs from 2026-10-14T03:00Z (inclusive) to 2026-10-15T03:00Z (exclusive)
    private static readonly Row[] Rows =
    [
        new("13 tarde da noite", new DateTime(2026, 10, 14, 2, 59, 59, DateTimeKind.Utc)),
        new("14 início", new DateTime(2026, 10, 14, 3, 0, 0, DateTimeKind.Utc)),
        new("14 fim", new DateTime(2026, 10, 15, 2, 59, 59, DateTimeKind.Utc)),
        new("15 início", new DateTime(2026, 10, 15, 3, 0, 0, DateTimeKind.Utc))
    ];

    [Fact]
    public void WhereDateInRange_Should_Include_Whole_Brazilian_Days()
    {
        // Arrange
        var day = new DateOnly(2026, 10, 14);

        // Act
        var result = Rows.AsQueryable().WhereDateInRange(r => r.CreatedAtUtc, day, day);

        // Assert
        result.Select(r => r.Name).ShouldBe(["14 início", "14 fim"]);
    }

    [Fact]
    public void WhereDateInRange_Should_Not_Filter_When_Dates_Are_Null()
    {
        // Act
        var result = Rows.AsQueryable().WhereDateInRange(r => r.CreatedAtUtc, null, null);

        // Assert
        result.Count().ShouldBe(Rows.Length);
    }

    [Fact]
    public void WhereDateInRange_Should_Apply_Open_Ended_Ranges()
    {
        // Arrange
        var day = new DateOnly(2026, 10, 14);

        // Act
        var fromDay = Rows.AsQueryable().WhereDateInRange(r => r.CreatedAtUtc, day, null);
        var untilDay = Rows.AsQueryable().WhereDateInRange(r => r.CreatedAtUtc, null, day);

        // Assert
        fromDay.Select(r => r.Name).ShouldBe(["14 início", "14 fim", "15 início"]);
        untilDay.Select(r => r.Name).ShouldBe(["13 tarde da noite", "14 início", "14 fim"]);
    }
}