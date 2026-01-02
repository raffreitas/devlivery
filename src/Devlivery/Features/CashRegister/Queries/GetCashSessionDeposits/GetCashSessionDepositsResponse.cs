using Devlivery.Features.CashRegister.Domain.Entities;
using Devlivery.Features.CashRegister.Domain.Enums;
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
    public static GetCashSessionDepositsResponse FromDomain(CashSessionMovement m)
    {
        return new GetCashSessionDepositsResponse(
            m.Id,
            m.CashSessionId,
            m.CreatedBy,
            string.Empty, // TODO: Buscar o nome do atendente
            m.Amount,
            m.CreatedAt,
            m.Reason);
    }
}