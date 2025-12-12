namespace Devlivery.Features.CashRegister.DTOs;

public sealed record CashDepositResponse(
    Guid Id,
    Guid CashSessionId,
    Guid AttendantId,
    string AttendantName,
    decimal Amount,
    DateTime DepositedAt,
    string? Notes)
{
    public static CashDepositResponse FromDomain(Domain.CashDeposit deposit)
    {
        return new CashDepositResponse(
            deposit.Id,
            deposit.CashSessionId,
            deposit.AttendantId,
            deposit.AttendantName,
            deposit.Amount,
            deposit.DepositedAt,
            deposit.Notes);
    }
}