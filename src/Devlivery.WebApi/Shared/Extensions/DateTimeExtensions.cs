using System.Linq.Expressions;

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

    /// <summary>
    /// Applies date range filtering to a queryable using Brazilian timezone conversion.
    /// Converts local dates (YYYY-MM-DD) to UTC ranges for database queries.
    /// Uses inclusive start (&gt;=) and exclusive end (&lt;) for proper day boundary handling.
    /// </summary>
    /// <typeparam name="T">The entity type being queried</typeparam>
    /// <param name="query">The queryable to filter</param>
    /// <param name="dateSelector">Expression selecting the DateTime property to filter on (e.g., o => o.CreatedAt)</param>
    /// <param name="startDate">Optional start date (inclusive, interpreted as Brazil local time)</param>
    /// <param name="endDate">Optional end date (inclusive, interpreted as Brazil local time)</param>
    /// <returns>Filtered queryable with date range applied</returns>
    /// <example>
    /// query.WhereDateInRange(o => o.CreatedAt, startDate, endDate)
    /// </example>
    public static IQueryable<T> WhereDateInRange<T>(
        this IQueryable<T> query,
        Expression<Func<T, DateTime>> dateSelector,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        if (startDate.HasValue)
        {
            var startUtc = startDate.Value.ToBrazilStartOfDayUtc();
            var parameter = dateSelector.Parameters[0];
            var property = dateSelector.Body;
            var comparison = Expression.GreaterThanOrEqual(property, Expression.Constant(startUtc));
            var lambda = Expression.Lambda<Func<T, bool>>(comparison, parameter);
            query = query.Where(lambda);
        }

        if (endDate.HasValue)
        {
            var endExclusiveUtc = endDate.Value.ToBrazilEndOfDayExclusiveUtc();
            var parameter = dateSelector.Parameters[0];
            var property = dateSelector.Body;
            var comparison = Expression.LessThan(property, Expression.Constant(endExclusiveUtc));
            var lambda = Expression.Lambda<Func<T, bool>>(comparison, parameter);
            query = query.Where(lambda);
        }

        return query;
    }
}