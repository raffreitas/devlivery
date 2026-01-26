using System.Data;

using Dapper;

using Devlivery.Domain.Aggregates.Expenses.Enums;
using Devlivery.Infrastructure.Persistence.Abstractions;
using Devlivery.Infrastructure.Tenancy;
using Devlivery.Infrastructure.Time.Abstractions;

using FluentResults;

using Mediator;

namespace Devlivery.Features.Expenses.Queries.GetAllExpenses;

public sealed class GetAllExpensesHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantAccessor tenantAccessor,
    IDateTimeProvider dateTimeProvider
)
    : IQueryHandler<GetAllExpensesQuery, Result<List<GetAllExpensesResponse>>>
{
    public async ValueTask<Result<List<GetAllExpensesResponse>>> Handle(GetAllExpensesQuery query,
        CancellationToken cancellationToken)
    {
        var today = dateTimeProvider.GetLocalDate();

        const string sql =
            $"""
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
             and (@CategoryId is null or ecp.id = @CategoryId)
               and (
                   @StatusFilter is null 
                   or (@StatusFilter = '{nameof(ExpenseDisplayStatus.Overdue)}' 
                           and e.status = '{nameof(ExpenseDisplayStatus.Pending)}' 
                           and e.due_date < @Today)
                   or (@StatusFilter = '{nameof(ExpenseDisplayStatus.DueToday)}' 
                           and e.status = '{nameof(ExpenseDisplayStatus.Pending)}' 
                           and e.due_date = @Today)
                   or (@StatusFilter not in ('{nameof(ExpenseDisplayStatus.Overdue)}', '{nameof(ExpenseDisplayStatus.DueToday)}') 
                           and e.status = @StatusFilter)
               )
               and (@StartDate is null or e.due_date >= @StartDate)
               and (@EndDate is null or e.due_date <= @EndDate)
             """;

        var parameters = new DynamicParameters();
        parameters.Add("EstablishmentId", tenantAccessor.Tenant.Id, DbType.Guid);
        parameters.Add("CategoryId", query.CategoryId, DbType.Guid);
        parameters.Add("StatusFilter", query.Status?.ToString(), DbType.String);
        parameters.Add("Today", today, DbType.Date);
        parameters.Add("StartDate", query.StartDate, DbType.Date);
        parameters.Add("EndDate", query.EndDate, DbType.Date);

        await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);
        var expenses = await connection.QueryAsync<GetAllExpensesQueryDto>(sql, parameters);

        return expenses
            .Select(e =>
            {
                var category = e.ParentCategoryId is not null
                    ? new CategoryDto(
                        e.ParentCategoryId.Value,
                        e.ParentCategoryName!,
                        e.ParentCategoryIsActive!.Value,
                        [
                            new CategoryDto(e.CategoryId, e.CategoryName, e.CategoryIsActive, [])
                        ])
                    : new CategoryDto(e.CategoryId, e.CategoryName, e.CategoryIsActive, []);

                var displayStatus = e.Status switch
                {
                    nameof(ExpenseStatus.Paid) => ExpenseDisplayStatus.Paid,
                    nameof(ExpenseStatus.Cancelled) => ExpenseDisplayStatus.Cancelled,
                    nameof(ExpenseStatus.Pending) when e.DueDate < today => ExpenseDisplayStatus.Overdue,
                    nameof(ExpenseStatus.Pending) when e.DueDate == today => ExpenseDisplayStatus.DueToday,
                    _ => ExpenseDisplayStatus.Pending
                };

                return new GetAllExpensesResponse(
                    e.Id,
                    category,
                    e.Supplier,
                    e.Description,
                    e.Amount,
                    e.DueDate,
                    e.PaymentDate,
                    displayStatus,
                    e.CreatedAt,
                    e.UpdatedAt);
            })
            .OrderBy(e => StatusSortingValue(e.Status))
            .ThenBy(e => e.DueDate)
            .ToList();
    }

    private static int StatusSortingValue(ExpenseDisplayStatus status) => status switch
    {
        ExpenseDisplayStatus.Overdue => 0,
        ExpenseDisplayStatus.DueToday => 1,
        ExpenseDisplayStatus.Pending => 2,
        ExpenseDisplayStatus.Paid => 3,
        _ => 4
    };
}