using System.Data;

using Dapper;

using Devlivery.Features.Expenses.Domain.Aggregates.Expenses.Enums;
using Devlivery.Infrastructure.Persistence.Abstractions;
using Devlivery.Infrastructure.Tenancy;

using FluentResults;

using Mediator;

namespace Devlivery.Features.Dashboard.Queries.GetExpensesOverTime;

public sealed class GetExpensesOverTimeHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantAccessor tenantAccessor)
    : IQueryHandler<GetExpensesOverTimeQuery, Result<GetExpensesOverTimeResponse>>
{
    public async ValueTask<Result<GetExpensesOverTimeResponse>> Handle(
        GetExpensesOverTimeQuery query,
        CancellationToken cancellationToken)
    {
        const string sql = """
            select 
                e.payment_date as "PaymentDate",
                sum(e.amount) as "Total"
            from public.expenses e
            where e.establishment_id = @EstablishmentId
            and e.status = @PaidStatus
            and e.payment_date is not null
            and (@StartDate is null or e.payment_date >= @StartDate)
            and (@EndDate is null or e.payment_date <= @EndDate)
            group by e.payment_date
            order by e.payment_date
            """;

        var parameters = new DynamicParameters();
        parameters.Add("EstablishmentId", tenantAccessor.Tenant.Id, DbType.Guid);
        parameters.Add("PaidStatus", nameof(ExpenseStatus.Paid), DbType.String);
        parameters.Add("StartDate", query.StartDate, DbType.Date);
        parameters.Add("EndDate", query.EndDate, DbType.Date);

        await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);
        var expensesByDate = await connection.QueryAsync<(DateOnly PaymentDate, decimal Total)>(sql, parameters);

        // Format dates as ISO date string (yyyy-MM-dd)
        var data = expensesByDate
            .Select(x => new ExpenseTimeSeriesItem(
                x.PaymentDate.ToString("yyyy-MM-dd"),
                x.Total))
            .ToList();

        var response = new GetExpensesOverTimeResponse(data);

        return Result.Ok(response);
    }
}