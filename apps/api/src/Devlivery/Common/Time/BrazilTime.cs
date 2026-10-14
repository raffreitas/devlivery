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
}