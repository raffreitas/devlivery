using System.Data;

using Dapper;

using Devlivery.Features.Expenses.Domain.Aggregates.Expenses.Enums;
using Devlivery.Infrastructure.Persistence.Abstractions;
using Devlivery.Infrastructure.Tenancy;
using Devlivery.Infrastructure.Time.Abstractions;

using FluentResults;

using Mediator;

namespace Devlivery.Features.Dashboard.Queries.GetExpenseSummary;

public sealed class GetExpenseSummaryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantAccessor tenantAccessor,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetExpenseSummaryQuery, Result<GetExpenseSummaryResponse>>
{
    public async ValueTask<Result<GetExpenseSummaryResponse>> Handle(
        GetExpenseSummaryQuery query,
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

        var expensesList = expenses.ToList();

        var total = expensesList.Sum(e => e.Amount);
        var paid = expensesList
            .Where(e => e.Status == nameof(ExpenseStatus.Paid))
            .Sum(e => e.Amount);
        var pending = expensesList
            .Where(e => e.Status == nameof(ExpenseStatus.Pending) && e.DueDate >= today)
            .Sum(e => e.Amount);
        var overdue = expensesList
            .Where(e => e.Status == nameof(ExpenseStatus.Pending) && e.DueDate < today)
            .Sum(e => e.Amount);
        var count = expensesList.Count;

        var response = new GetExpenseSummaryResponse(total, paid, pending, overdue, count);

        return Result.Ok(response);
    }
}