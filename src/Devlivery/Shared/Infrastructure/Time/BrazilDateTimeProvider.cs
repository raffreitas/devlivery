using Devlivery.Shared.Application.Abstractions;

namespace Devlivery.Shared.Infrastructure.Time;

public sealed class BrazilDateTimeProvider : IDateTimeProvider
{
    private static readonly TimeZoneInfo BrazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public DateTime UtcNow => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BrazilTimeZone);
    public DateTime GetLocalNow() => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BrazilTimeZone);
    public DateOnly GetLocalDate() => DateOnly.FromDateTime(GetLocalNow());
}