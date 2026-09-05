using Devlivery.Domain.Aggregates.CashRegister.Entities;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessionDeposits;

public sealed record GetCashSessionDepositsResponse(
    Guid Id,
    Guid CashSessionId,
    Guid AttendantId,
    string AttendantName,
    decimal Amount,
    DateTime DepositedAt,
    string? Notes)
{
    public static GetCashSessionDepositsResponse FromDomain(CashSessionMovement m, string authorName)
    {
        return new GetCashSessionDepositsResponse(
            m.Id,
            m.CashSessionId,
            m.CreatedBy,
            authorName,
            m.Amount,
            m.CreatedAt,
            m.Reason);
    }
}