using System.Data;

using Dapper;

using Devlivery.Common.Handlers;
using Devlivery.Common.Time;
using Devlivery.Features.Dashboard.Common;
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

        const string sql = $"""
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
            {ExpenseDashboardQuery.WhereClause}
            group by 1
            """;

        var parameters = ExpenseDashboardQuery.CreateParameters(
            tenantAccessor.Tenant.Id, query.StartDate, query.EndDate);
        parameters.Add("Today", today, DbType.Date);
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
