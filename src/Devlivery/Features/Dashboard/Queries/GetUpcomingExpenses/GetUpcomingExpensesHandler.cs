using System.Data;

using Dapper;

using Devlivery.Features.Expenses.Domain.Aggregates.Expenses.Enums;
using Devlivery.Features.Expenses.Queries.GetAllExpenses;
using Devlivery.Shared.Application.Abstractions;
using Devlivery.Shared.Infrastructure.Persistence.Abstractions;
using Devlivery.Shared.Infrastructure.Tenancy;

using FluentResults;

using Mediator;

namespace Devlivery.Features.Dashboard.Queries.GetUpcomingExpenses;

public sealed class GetUpcomingExpensesHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantAccessor tenantAccessor,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetUpcomingExpensesQuery, Result<GetUpcomingExpensesResponse>>
{
    public async ValueTask<Result<GetUpcomingExpensesResponse>> Handle(
        GetUpcomingExpensesQuery query,
        CancellationToken cancellationToken)
    {
        var today = dateTimeProvider.GetLocalDate();
        var futureDate = today.AddDays(query.Days);

        const string sql = """
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
            and e.status not in (@PaidStatus, @CancelledStatus)
            and e.due_date >= @Today
            and e.due_date <= @FutureDate
            order by e.due_date
            """;

        var parameters = new DynamicParameters();
        parameters.Add("EstablishmentId", tenantAccessor.Tenant.Id, DbType.Guid);
        parameters.Add("PaidStatus", nameof(ExpenseStatus.Paid), DbType.String);
        parameters.Add("CancelledStatus", nameof(ExpenseStatus.Cancelled), DbType.String);
        parameters.Add("Today", today, DbType.Date);
        parameters.Add("FutureDate", futureDate, DbType.Date);

        await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);
        var expenses = await connection.QueryAsync<GetAllExpensesQueryDto>(sql, parameters);

        var expensesList = expenses
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

                return new UpcomingExpenseItem(
                    e.Id,
                    category,
                    e.Supplier,
                    e.Description,
                    e.Amount,
                    e.DueDate,
                    displayStatus);
            })
            .ToList();

        var response = new GetUpcomingExpensesResponse(expensesList);

        return Result.Ok(response);
    }
}