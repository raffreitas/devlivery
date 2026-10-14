namespace Devlivery.Domain.SeedWork;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}