using System.Data;

using Dapper;

using Devlivery.Common.Handlers;
using Devlivery.Common.Time;
using Devlivery.Domain.Aggregates.Expenses.Enums;
using Devlivery.Features.Expenses.Queries.GetAllExpenses;
using Devlivery.Infrastructure.Persistence.Abstractions;
using Devlivery.Infrastructure.Tenancy;

using FluentResults;

namespace Devlivery.Features.Dashboard.Queries.GetExpensesByStatus;

public sealed class GetExpensesByStatusHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantAccessor tenantAccessor,
    TimeProvider timeProvider)
    : IQueryHandler<GetExpensesByStatusQuery, Result<GetExpensesByStatusResponse>>
{
    public async Task<Result<GetExpensesByStatusResponse>> HandleAsync(
        GetExpensesByStatusQuery query,
        CancellationToken cancellationToken)
    {
        var today = timeProvider.GetBrazilToday();

        const string sql = """
            select
                case
                    when e.status = @Paid then @PaidDisplay
                    when e.due_date < @Today then @OverdueDisplay
                    when e.due_date = @Today then @DueTodayDisplay
                    else @PendingDisplay
                end as "Status",
                count(*)::int as "Count",
                sum(e.amount) as "Total"
            from public.expenses e
            where e.establishment_id = @EstablishmentId
            and e.status != @Cancelled
            and (@StartDate is null or (case when e.status = @Paid then e.payment_date else e.due_date end) >= @StartDate)
            and (@EndDate is null or (case when e.status = @Paid then e.payment_date else e.due_date end) <= @EndDate)
            group by 1
            """;

        var parameters = new DynamicParameters();
        parameters.Add("EstablishmentId", tenantAccessor.Tenant.Id, DbType.Guid);
        parameters.Add("StartDate", query.StartDate, DbType.Date);
        parameters.Add("EndDate", query.EndDate, DbType.Date);
        parameters.Add("Today", today, DbType.Date);
        parameters.Add("Paid", nameof(ExpenseStatus.Paid));
        parameters.Add("Cancelled", nameof(ExpenseStatus.Cancelled));
        parameters.Add("PaidDisplay", nameof(ExpenseDisplayStatus.Paid));
        parameters.Add("OverdueDisplay", nameof(ExpenseDisplayStatus.Overdue));
        parameters.Add("DueTodayDisplay", nameof(ExpenseDisplayStatus.DueToday));
        parameters.Add("PendingDisplay", nameof(ExpenseDisplayStatus.Pending));

        await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);
        var totals = await connection.QueryAsync<(string Status, int Count, decimal Total)>(sql, parameters);

        var statusGroups = totals
            .Select(t => new ExpenseStatusItem(Enum.Parse<ExpenseDisplayStatus>(t.Status), t.Count, t.Total))
            .OrderBy(s => s.Status.ToString())
            .ToList();

        var response = new GetExpensesByStatusResponse(statusGroups);

        return Result.Ok(response);
    }
}
