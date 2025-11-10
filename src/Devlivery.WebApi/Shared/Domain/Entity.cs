namespace Devlivery.WebApi.Shared.Domain;

public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.CreateVersion7();
}