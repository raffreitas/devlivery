namespace Devlivery.WebApi.Features.CashRegister.Commands.CreateCashSession;

public sealed record CreateCashSessionResponse(
    Guid Id,
    string AttendantName,
    decimal OpeningAmount,
    DateTime StartAt,
    string Status);
