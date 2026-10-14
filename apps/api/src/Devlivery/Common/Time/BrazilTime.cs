namespace Devlivery.Common.Time;

/// <summary>
/// Single source for the business time zone. Dates are stored in UTC; "today" and
/// day boundaries are defined by the Brazilian local time (America/Sao_Paulo).
/// </summary>
public static class BrazilTime
{
    public static readonly TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public static DateTime GetBrazilNow(this TimeProvider timeProvider) =>
        TimeZoneInfo.ConvertTimeFromUtc(timeProvider.GetUtcNow().UtcDateTime, TimeZone);

    public static DateOnly GetBrazilToday(this TimeProvider timeProvider) =>
        DateOnly.FromDateTime(timeProvider.GetBrazilNow());

    public static DateTime ToBrazilTime(this DateTime utcDateTime) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc), TimeZone);

    /// <summary>
    /// UTC instant when the given Brazilian calendar day starts (00:00 in São Paulo).
    /// </summary>
    public static DateTime ToBrazilStartOfDayUtc(this DateOnly date) =>
        TimeZoneInfo.ConvertTimeToUtc(date.ToDateTime(TimeOnly.MinValue), TimeZone);

    /// <summary>
    /// UTC instant when the given Brazilian calendar day ends, exclusive (00:00 of the next day in São Paulo).
    /// </summary>
    public static DateTime ToBrazilEndOfDayExclusiveUtc(this DateOnly date) =>
        date.AddDays(1).ToBrazilStartOfDayUtc();
}