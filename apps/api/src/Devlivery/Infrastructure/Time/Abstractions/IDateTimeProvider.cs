namespace Devlivery.Infrastructure.Time.Abstractions;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateTime GetLocalNow();
    DateOnly GetLocalDate();
}