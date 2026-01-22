using Devlivery.Domain.Common.Enums;

namespace Devlivery.Features.Dashboard.Queries.GetPaymentBreakdown;

public sealed record GetPaymentBreakdownResponse(
    Dictionary<PaymentMethod, decimal> Breakdown,
    decimal Total);