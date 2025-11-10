using Devlivery.WebApi.Shared.Domain;

namespace Devlivery.WebApi.Features.Users.Domain;

public class User : Entity
{
    public string Name { get; private set; }
    public string Email { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public User(string name, string email)
    {
        Name = name;
        Email = email;
    }
}