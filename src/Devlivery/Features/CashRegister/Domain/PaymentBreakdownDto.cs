namespace Devlivery.Features.CashRegister.Domain;

public sealed record PaymentBreakdownDto(string Method, decimal Amount, int Count);

