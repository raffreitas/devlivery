using System.Data;

using Dapper;

using Devlivery.Common.Handlers;
using Devlivery.Domain.Aggregates.Expenses.Enums;
using Devlivery.Infrastructure.Persistence.Abstractions;
using Devlivery.Infrastructure.Tenancy;

using FluentResults;

namespace Devlivery.Features.Dashboard.Queries.GetExpensesByCategory;

public sealed class GetExpensesByCategoryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantAccessor tenantAccessor)
    : IQueryHandler<GetExpensesByCategoryQuery, Result<GetExpensesByCategoryResponse>>
{
    public async Task<Result<GetExpensesByCategoryResponse>> HandleAsync(
        GetExpensesByCategoryQuery query,
        CancellationToken cancellationToken)
    {
        const string sql = """
            select 
                coalesce(ecp.name, ec.name) as "CategoryName",
                sum(e.amount) as "Total"
            from public.expenses e
            join public.expense_categories ec on ec.id = e.category_id
            left join public.expense_categories ecp on ecp.id = ec.parent_category_id
            where e.establishment_id = @EstablishmentId
            and e.status != @Cancelled
            and (@StartDate is null or (case when e.status = @Paid then e.payment_date else e.due_date end) >= @StartDate)
            and (@EndDate is null or (case when e.status = @Paid then e.payment_date else e.due_date end) <= @EndDate)
            group by coalesce(ecp.name, ec.name)
            order by sum(e.amount) desc
            """;

        var parameters = new DynamicParameters();
        parameters.Add("EstablishmentId", tenantAccessor.Tenant.Id, DbType.Guid);
        parameters.Add("StartDate", query.StartDate, DbType.Date);
        parameters.Add("EndDate", query.EndDate, DbType.Date);
        parameters.Add("Paid", nameof(ExpenseStatus.Paid));
        parameters.Add("Cancelled", nameof(ExpenseStatus.Cancelled));

        await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);
        var categoryTotals = await connection.QueryAsync<(string CategoryName, decimal Total)>(sql, parameters);

        var categoriesList = categoryTotals.ToList();
        var total = categoriesList.Sum(c => c.Total);

        var categories = categoriesList
            .Select(c => new ExpenseCategoryItem(
                c.CategoryName,
                c.Total,
                total > 0 ? (c.Total / total) * 100 : 0))
            .ToList();

        var response = new GetExpensesByCategoryResponse(categories);

        return Result.Ok(response);
    }
}
