namespace Devlivery.Shared.Application.Abstractions;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateTime GetLocalNow();
    DateOnly GetLocalDate();
}