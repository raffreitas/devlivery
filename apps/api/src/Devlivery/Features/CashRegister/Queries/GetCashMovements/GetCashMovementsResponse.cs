using Devlivery.Domain.Common.Enums;

namespace Devlivery.Features.CashRegister.Queries.GetCashMovements;

public sealed record GetCashMovementsResponse(
    string Id,
    DateTime CreatedAt,
    string Type,
    decimal SignedAmount,
    PaymentMethod? Method,
    Guid? RelatedOrderId,
    Guid? OrderPaymentId,
    Guid CreatedBy,
    string AuthorName,
    string? Reason);