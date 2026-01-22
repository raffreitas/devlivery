using Devlivery.Common.SeedWork;

namespace Devlivery.Features.Users.Domain;

public class User : Entity
{
    public string Name { get; private set; }
    public string Email { get; private set; }
    public Guid EstablishmentId { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public User(string name, string email, Guid establishmentId)
    {
        Name = name;
        Email = email;
        EstablishmentId = establishmentId;
    }
}