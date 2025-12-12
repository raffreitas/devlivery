namespace Devlivery.Shared.SeedWork;

public abstract class Entity
{
    public Guid Id { get; protected init; } = Guid.CreateVersion7();
}