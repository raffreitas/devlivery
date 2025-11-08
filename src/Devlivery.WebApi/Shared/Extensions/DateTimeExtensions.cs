namespace Devlivery.WebApi.Shared.Extensions;

public static class DateTimeExtensions
{
    private static readonly TimeZoneInfo BrazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    /// <summary>
    /// Converts a local Brazil date (BRT/BRST) to UTC start of day (00:00:00 local time → UTC).
    /// Use this for date range filtering when dates come from Brazilian timezone.
    /// </summary>
    public static DateTime ToBrazilStartOfDayUtc(this DateTime date)
    {
        var localStart = DateTime.SpecifyKind(date.Date, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(localStart, BrazilTimeZone);
    }

    /// <summary>
    /// Nullable overload returning null when input is null.
    /// </summary>
    public static DateTime? ToBrazilStartOfDayUtc(this DateTime? date) =>
        date?.ToBrazilStartOfDayUtc();

    /// <summary>
    /// Converts a local Brazil date (BRT/BRST) to UTC end of day exclusive (next day 00:00:00 local time → UTC).
    /// Use this for date range filtering when dates come from Brazilian timezone.
    /// </summary>
    public static DateTime ToBrazilEndOfDayExclusiveUtc(this DateTime date)
    {
        var localEnd = DateTime.SpecifyKind(date.Date.AddDays(1), DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(localEnd, BrazilTimeZone);
    }

    /// <summary>
    /// Nullable overload returning null when input is null.
    /// </summary>
    public static DateTime? ToBrazilEndOfDayExclusiveUtc(this DateTime? date) =>
        date?.ToBrazilEndOfDayExclusiveUtc();
}