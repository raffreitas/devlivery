using System.Data;

using Dapper;

using Devlivery.Common.Errors;
using Devlivery.Common.Handlers;
using Devlivery.Common.Pagination;
using Devlivery.Common.Time;
using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Domain.Common.Enums;
using Devlivery.Infrastructure.Persistence.Abstractions;
using Devlivery.Infrastructure.Tenancy;

using FluentResults;

namespace Devlivery.Features.CashRegister.Queries.GetCashHistory;

public sealed class GetCashHistoryHandler(IDbConnectionFactory dbConnectionFactory, ITenantAccessor tenantAccessor)
    : IQueryHandler<GetCashHistoryQuery, Result<GetCashHistoryResult>>
{
    public async Task<Result<GetCashHistoryResult>> HandleAsync(GetCashHistoryQuery query,
        CancellationToken cancellationToken)
    {
        var error = Validate(query);
        if (error is not null) return Result.Fail(error);

        var parameters = new DynamicParameters();
        parameters.Add("EstablishmentId", tenantAccessor.Tenant.Id, DbType.Guid);
        parameters.Add("StartAt", query.Start.ToBrazilStartOfDayUtc(), DbType.DateTime);
        parameters.Add("EndAt", query.End.ToBrazilEndOfDayExclusiveUtc(), DbType.DateTime);
        parameters.Add("Offset", (query.Page - 1) * query.PageSize, DbType.Int32);
        parameters.Add("Limit", query.PageSize, DbType.Int32);
        parameters.Add("Open", nameof(CashSessionStatus.Open), DbType.String);
        parameters.Add("Closed", nameof(CashSessionStatus.Closed), DbType.String);
        parameters.Add("Payment", nameof(CashSessionEntryType.Payment), DbType.String);
        parameters.Add("Refund", nameof(CashSessionEntryType.Refund), DbType.String);
        parameters.Add("Change", nameof(CashSessionEntryType.Change), DbType.String);
        parameters.Add("Deposit", nameof(CashSessionEntryType.Deposit), DbType.String);
        parameters.Add("Cash", nameof(PaymentMethod.Cash), DbType.String);

        await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);
        using var results = await connection.QueryMultipleAsync(new CommandDefinition(
            GetCashHistorySql.Query, parameters, cancellationToken: cancellationToken));
        var summary = await results.ReadSingleAsync<CashHistorySummary>();
        var sessions = (await results.ReadAsync<CashHistorySession>()).ToArray();

        return Result.Ok(new GetCashHistoryResult(
            new PaginatedResult<CashHistorySession>(sessions, summary.TotalCount, query.Page, query.PageSize),
            summary.ToTotals()));
    }

    private static ValidationError? Validate(GetCashHistoryQuery query)
    {
        if (query.Start > query.End || query.End == DateOnly.MaxValue)
            return new ValidationError("Informe um período válido, com início anterior ou igual ao fim.");
        return null;
    }
}
