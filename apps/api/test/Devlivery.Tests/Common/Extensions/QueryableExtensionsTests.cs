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

    private static readonly DateOnly Day = new(2026, 10, 14);

    [Fact]
    public void GivenSingleDayRange_WhenFiltering_ThenIncludesWholeBrazilianDay()
    {
        // Arrange
        var rows = Rows.AsQueryable();

        // Act
        var result = rows.WhereDateInRange(r => r.CreatedAtUtc, Day, Day);

        // Assert
        result.Select(r => r.Name).ShouldBe(["14 início", "14 fim"]);
    }

    [Fact]
    public void GivenNoDates_WhenFiltering_ThenKeepsAllRows()
    {
        // Arrange
        var rows = Rows.AsQueryable();

        // Act
        var result = rows.WhereDateInRange(r => r.CreatedAtUtc, null, null);

        // Assert
        result.Count().ShouldBe(Rows.Length);
    }

    [Fact]
    public void GivenOnlyStartDate_WhenFiltering_ThenIncludesEverythingFromStartOfThatDay()
    {
        // Arrange
        var rows = Rows.AsQueryable();

        // Act
        var result = rows.WhereDateInRange(r => r.CreatedAtUtc, Day, null);

        // Assert
        result.Select(r => r.Name).ShouldBe(["14 início", "14 fim", "15 início"]);
    }

    [Fact]
    public void GivenOnlyEndDate_WhenFiltering_ThenIncludesEverythingUntilEndOfThatDay()
    {
        // Arrange
        var rows = Rows.AsQueryable();

        // Act
        var result = rows.WhereDateInRange(r => r.CreatedAtUtc, null, Day);

        // Assert
        result.Select(r => r.Name).ShouldBe(["13 tarde da noite", "14 início", "14 fim"]);
    }
}