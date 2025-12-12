using Bogus;
using Devlivery.Features.Users.Domain;

namespace Devlivery.Tests.Common.Builders;

public class UserBuilder
{
    private readonly Faker _faker = new();

    private string _name;
    private string _email;
    private Guid _establishmentId;

    public UserBuilder()
    {
        _name = _faker.Name.FullName();
        _email = _faker.Internet.Email();
    }

    public UserBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithEstablishmentId(Guid establishmentId)
    {
        _establishmentId = establishmentId;
        return this;
    }

    public User Build()
    {
        return _establishmentId == Guid.Empty
            ? throw new InvalidOperationException("EstablishmentId must be set")
            : new User(_name, _email, _establishmentId);
    }
}