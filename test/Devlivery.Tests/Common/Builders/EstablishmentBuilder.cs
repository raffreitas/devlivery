using Bogus;
using Devlivery.Features.Establishments.Domain;

namespace Devlivery.Tests.Common.Builders;

public sealed class EstablishmentBuilder
{
    private readonly Faker _faker = new();

    private string _tradeName;
    private bool _isActive;

    public EstablishmentBuilder()
    {
        _tradeName = _faker.Company.CompanyName();
        _isActive = true;
    }

    public EstablishmentBuilder WithTradeName(string tradeName)
    {
        _tradeName = tradeName;
        return this;
    }

    public EstablishmentBuilder IsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public Establishment Build()
    {
        return new Establishment(_tradeName, _isActive);
    }
}