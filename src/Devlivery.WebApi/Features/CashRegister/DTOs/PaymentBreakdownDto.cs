namespace Devlivery.WebApi.Features.CashRegister.DTOs;

public sealed record PaymentBreakdownDto(string Method, decimal Amount, int Count);