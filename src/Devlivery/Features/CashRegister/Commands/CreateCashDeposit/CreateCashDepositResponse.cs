namespace Devlivery.Features.CashRegister.Commands.CreateCashDeposit;

public sealed record CreateCashDepositResponse(
    Guid Id,
    decimal Amount,
    string AttendantName,
    DateTime CreatedAt);