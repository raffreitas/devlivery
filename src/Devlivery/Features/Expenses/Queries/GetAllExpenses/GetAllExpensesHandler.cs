using System.Data;

using Dapper;

using Devlivery.Shared.Infrastructure.Persistence.Abstractions;
using Devlivery.Shared.Infrastructure.Tenancy;

using Mediator;

namespace Devlivery.Features.Expenses.Queries.GetAllExpenses;

public sealed class GetAllExpensesHandler(IDbConnectionFactory dbConnectionFactory, ITenantAccessor tenantAccessor)
    : IQueryHandler<GetAllExpensesQuery, List<GetAllExpensesResponse>>
{
    public async ValueTask<List<GetAllExpensesResponse>> Handle(GetAllExpensesQuery query,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            select e.id               as "Id",
                   e.establishment_id as "EstablishmentId",
                   e.supplier         as "Supplier",
                   e.description      as "Description",
                   e.amount           as "Amount",
                   e.due_date         as "DueDate",
                   e.payment_date     as "PaymentDate",
                   e.status           as "Status",
                   e.created_at       as "CreatedAt",
                   e.updated_at       as "UpdatedAt",
                   ec.id              as "CategoryId",
                   ec.name            as "CategoryName",
                   ec.is_active       as "CategoryIsActive",
                   ecp.id             as "ParentCategoryId",
                   ecp.name           as "ParentCategoryName",
                   ecp.is_active      as "ParentCategoryIsActive"
            from public.expenses e
                     join public.expense_categories ec on ec.id = e.category_id
                     left join public.expense_categories ecp on ecp.id = ec.parent_category_id
            where e.establishment_id = @EstablishmentId
              and (@CategoryId is null or e.category_id = @CategoryId)
              and (@Status is null or e.status = @Status)
              and (@StartDate is null or e.due_date >= @StartDate)
              and (@EndDate is null or e.due_date <= @EndDate)
            """;

        var parameters = new DynamicParameters();
        parameters.Add("EstablishmentId", tenantAccessor.Tenant.Id, DbType.Guid);
        parameters.Add("CategoryId", query.CategoryId, DbType.Guid);
        parameters.Add("Status", query.Status?.ToString(), DbType.String);
        parameters.Add("StartDate", query.StartDate, DbType.Date);
        parameters.Add("EndDate", query.EndDate, DbType.Date);

        await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);
        var expenses = await connection.QueryAsync<GetAllExpensesQueryDto>(sql, parameters);

        return expenses
            .Select(e => new GetAllExpensesResponse(
                e.Id,
                new CategoryDto(
                    e.CategoryId,
                    e.CategoryName,
                    e.CategoryIsActive,
                    e.ParentCategoryId.HasValue
                        ?
                        [
                            new CategoryDto(
                                e.ParentCategoryId.Value,
                                e.ParentCategoryName!,
                                e.ParentCategoryIsActive!.Value,
                                [])
                        ]
                        : []),
                e.Supplier,
                e.Description,
                e.Amount,
                e.DueDate,
                e.PaymentDate,
                e.Status,
                e.CreatedAt,
                e.UpdatedAt))
            .ToList();
    }
}