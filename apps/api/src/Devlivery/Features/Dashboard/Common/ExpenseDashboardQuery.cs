using System.Data;

using Dapper;

using Devlivery.Domain.Aggregates.Expenses.Enums;

namespace Devlivery.Features.Dashboard.Common;

internal static class ExpenseDashboardQuery
{
    public const string WhereClause = """
        where e.establishment_id = @EstablishmentId
        and e.status != @Cancelled
        and (@StartDate is null or (case when e.status = @Paid then e.payment_date else e.due_date end) >= @StartDate)
        and (@EndDate is null or (case when e.status = @Paid then e.payment_date else e.due_date end) <= @EndDate)
        """;

    public static DynamicParameters CreateParameters(Guid establishmentId, DateOnly? startDate, DateOnly? endDate)
    {
        var parameters = new DynamicParameters();
        parameters.Add("EstablishmentId", establishmentId, DbType.Guid);
        parameters.Add("StartDate", startDate, DbType.Date);
        parameters.Add("EndDate", endDate, DbType.Date);
        parameters.Add("Paid", nameof(ExpenseStatus.Paid));
        parameters.Add("Cancelled", nameof(ExpenseStatus.Cancelled));

        return parameters;
    }
}
