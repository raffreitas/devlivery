using Devlivery.Features.CashRegister.Domain;

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
    public static GetCashSessionDepositsResponse FromDomain(CashDeposit deposit)
    {
        return new GetCashSessionDepositsResponse(
            deposit.Id,
            deposit.CashSessionId,
            deposit.AttendantId,
            deposit.AttendantName,
            deposit.Amount,
            deposit.DepositedAt,
            deposit.Notes);
    }
}

