namespace Devlivery.Features.CashRegister.Queries.GetCashMovements;

public sealed record GetCashMovementsQuery(Guid SessionId, int Page = 1, int PageSize = 50);