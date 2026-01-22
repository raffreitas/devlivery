using System.Data;

using Dapper;

using Devlivery.Domain.Aggregates.Expenses.Enums;
using Devlivery.Features.Expenses.Queries.GetAllExpenses;
using Devlivery.Infrastructure.Persistence.Abstractions;
using Devlivery.Infrastructure.Tenancy;
using Devlivery.Infrastructure.Time.Abstractions;

using FluentResults;

using Mediator;

namespace Devlivery.Features.Dashboard.Queries.GetExpensesByStatus;

public sealed class GetExpensesByStatusHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantAccessor tenantAccessor,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetExpensesByStatusQuery, Result<GetExpensesByStatusResponse>>
{
    public async ValueTask<Result<GetExpensesByStatusResponse>> Handle(
        GetExpensesByStatusQuery query,
        CancellationToken cancellationToken)
    {
        var today = dateTimeProvider.GetLocalDate();

        const string sql = """
            select 
                e.status as "Status",
                e.due_date as "DueDate",
                e.amount as "Amount"
            from public.expenses e
            where e.establishment_id = @EstablishmentId
            and (@StartDate is null or e.due_date >= @StartDate)
            and (@EndDate is null or e.due_date <= @EndDate)
            """;

        var parameters = new DynamicParameters();
        parameters.Add("EstablishmentId", tenantAccessor.Tenant.Id, DbType.Guid);
        parameters.Add("StartDate", query.StartDate, DbType.Date);
        parameters.Add("EndDate", query.EndDate, DbType.Date);
        parameters.Add("Today", today, DbType.Date);

        await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);
        var expenses = await connection.QueryAsync<(string Status, DateOnly DueDate, decimal Amount)>(sql, parameters);

        // Calculate display status and group
        var statusGroups = expenses
            .Select(e =>
            {
                var displayStatus = e.Status switch
                {
                    nameof(ExpenseStatus.Paid) => ExpenseDisplayStatus.Paid,
                    nameof(ExpenseStatus.Cancelled) => ExpenseDisplayStatus.Cancelled,
                    nameof(ExpenseStatus.Pending) when e.DueDate < today => ExpenseDisplayStatus.Overdue,
                    nameof(ExpenseStatus.Pending) when e.DueDate == today => ExpenseDisplayStatus.DueToday,
                    _ => ExpenseDisplayStatus.Pending
                };

                return new { Status = displayStatus, Amount = e.Amount };
            })
            .GroupBy(e => e.Status)
            .Select(g => new ExpenseStatusItem(
                g.Key,
                g.Count(),
                g.Sum(e => e.Amount)))
            .OrderBy(s => s.Status.ToString())
            .ToList();

        var response = new GetExpensesByStatusResponse(statusGroups);

        return Result.Ok(response);
    }
}