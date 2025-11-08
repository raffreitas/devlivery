namespace Devlivery.WebApi.Shared.Extensions;

public static class DateTimeExtensions
{
    /// <summary>
    /// Returns the start of the given date as UTC (00:00:00) — preserves Date part and sets Kind = Utc.
    /// </summary>
    public static DateTime ToUtcStartOfDay(this DateTime date) =>
        DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);

    /// <summary>
    /// Nullable overload returning null when input is null.
    /// </summary>
    public static DateTime? ToUtcStartOfDay(this DateTime? date) =>
        date.HasValue ? DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc) : null;

    /// <summary>
    /// Returns the exclusive end bound for the given date as UTC (next day 00:00:00) — useful for end-exclusive filters.
    /// </summary>
    public static DateTime ToUtcEndExclusiveOfDay(this DateTime date) =>
        DateTime.SpecifyKind(date.Date.AddDays(1), DateTimeKind.Utc);

    /// <summary>
    /// Nullable overload returning null when input is null.
    /// </summary>
    public static DateTime? ToUtcEndExclusiveOfDay(this DateTime? date) =>
        date.HasValue ? DateTime.SpecifyKind(date.Value.Date.AddDays(1), DateTimeKind.Utc) : null;
}