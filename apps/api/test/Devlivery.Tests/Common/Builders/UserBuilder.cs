using Bogus;

using Devlivery.Domain.Aggregates.Users;

namespace Devlivery.Tests.Common.Builders;

public sealed class UserBuilder
{
    private readonly Faker _faker = new("pt_BR");

    private string _name;
    private string _email;
    private Guid _establishmentId;

    public UserBuilder()
    {
        _name = _faker.Name.FullName();
        _email = _faker.Internet.Email();
        _establishmentId = Guid.NewGuid();
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
        return new User(_name, _email, _establishmentId);
    }
}