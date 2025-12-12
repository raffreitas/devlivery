namespace Devlivery.Features.CashRegister.DTOs;

public sealed record PaymentBreakdownDto(string Method, decimal Amount, int Count);