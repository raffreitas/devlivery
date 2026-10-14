using System.Linq.Expressions;

using Devlivery.Common.Time;

namespace Devlivery.Common.Extensions;

public static class QueryableExtensions
{
    /// <summary>
    /// Filters a UTC date column by a period of Brazilian calendar days.
    /// Both days are inclusive: the start uses <c>&gt;=</c> its first instant and the end uses
    /// <c>&lt;</c> the first instant of the following day.
    /// </summary>
    /// <example><c>query.WhereDateInRange(o => o.CreatedAt, startDate, endDate)</c></example>
    public static IQueryable<T> WhereDateInRange<T>(
        this IQueryable<T> query,
        Expression<Func<T, DateTime>> dateSelector,
        DateOnly? startDate,
        DateOnly? endDate)
    {
        if (startDate is { } start)
            query = query.Where(Compare(dateSelector, Expression.GreaterThanOrEqual, start.ToBrazilStartOfDayUtc()));

        if (endDate is { } end)
            query = query.Where(Compare(dateSelector, Expression.LessThan, end.ToBrazilEndOfDayExclusiveUtc()));

        return query;
    }

    private static Expression<Func<T, bool>> Compare<T>(
        Expression<Func<T, DateTime>> dateSelector,
        Func<Expression, Expression, BinaryExpression> comparison,
        DateTime utcBoundary) =>
        Expression.Lambda<Func<T, bool>>(
            comparison(dateSelector.Body, Expression.Constant(utcBoundary)),
            dateSelector.Parameters);
}