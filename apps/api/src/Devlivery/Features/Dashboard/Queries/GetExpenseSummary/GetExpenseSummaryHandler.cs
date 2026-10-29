using System.Data;

using Dapper;

using Devlivery.Common.Handlers;
using Devlivery.Common.Time;
using Devlivery.Domain.Aggregates.Expenses.Enums;
using Devlivery.Infrastructure.Persistence.Abstractions;
using Devlivery.Infrastructure.Tenancy;

using FluentResults;

namespace Devlivery.Features.Dashboard.Queries.GetExpenseSummary;

public sealed class GetExpenseSummaryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantAccessor tenantAccessor,
    TimeProvider timeProvider)
    : IQueryHandler<GetExpenseSummaryQuery, Result<GetExpenseSummaryResponse>>
{
    public async Task<Result<GetExpenseSummaryResponse>> HandleAsync(
        GetExpenseSummaryQuery query,
        CancellationToken cancellationToken)
    {
        var today = timeProvider.GetBrazilToday();

        const string sql = """
            select
                coalesce(sum(e.amount), 0) as "Total",
                coalesce(sum(e.amount) filter (where e.status = @Paid), 0) as "Paid",
                coalesce(sum(e.amount) filter (where e.status = @Pending and e.due_date >= @Today), 0) as "Pending",
                coalesce(sum(e.amount) filter (where e.status = @Pending and e.due_date < @Today), 0) as "Overdue",
                count(*)::int as "Count"
            from public.expenses e
            where e.establishment_id = @EstablishmentId
            and e.status != @Cancelled
            and (@StartDate is null or (case when e.status = @Paid then e.payment_date else e.due_date end) >= @StartDate)
            and (@EndDate is null or (case when e.status = @Paid then e.payment_date else e.due_date end) <= @EndDate)
            """;

        var parameters = new DynamicParameters();
        parameters.Add("EstablishmentId", tenantAccessor.Tenant.Id, DbType.Guid);
        parameters.Add("StartDate", query.StartDate, DbType.Date);
        parameters.Add("EndDate", query.EndDate, DbType.Date);
        parameters.Add("Today", today, DbType.Date);
        parameters.Add("Paid", nameof(ExpenseStatus.Paid));
        parameters.Add("Pending", nameof(ExpenseStatus.Pending));
        parameters.Add("Cancelled", nameof(ExpenseStatus.Cancelled));

        await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);
        var summary = await connection.QuerySingleAsync<(decimal Total, decimal Paid, decimal Pending, decimal Overdue, int Count)>(
            sql, parameters);

        var response = new GetExpenseSummaryResponse(
            summary.Total, summary.Paid, summary.Pending, summary.Overdue, summary.Count);

        return Result.Ok(response);
    }
}
