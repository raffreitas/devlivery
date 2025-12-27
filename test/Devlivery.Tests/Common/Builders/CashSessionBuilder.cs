using Bogus;

using Devlivery.Features.CashRegister.Domain;

namespace Devlivery.Tests.Common.Builders;

public sealed class CashSessionBuilder
{
    private readonly Faker _faker = new();

    private Guid _establishmentId;
    private Guid _attendantId;
    private string _attendantName;
    private decimal _openingAmount;
    private string? _notes;

    public CashSessionBuilder()
    {
        _establishmentId = Guid.NewGuid();
        _attendantId = Guid.NewGuid();
        _attendantName = _faker.Name.FullName();
        _openingAmount = _faker.Random.Decimal(0, 1000);
        _notes = null;
    }

    public CashSessionBuilder WithEstablishmentId(Guid establishmentId)
    {
        _establishmentId = establishmentId;
        return this;
    }

    public CashSessionBuilder WithAttendantId(Guid attendantId)
    {
        _attendantId = attendantId;
        return this;
    }

    public CashSessionBuilder WithAttendantName(string attendantName)
    {
        _attendantName = attendantName;
        return this;
    }

    public CashSessionBuilder WithOpeningAmount(decimal openingAmount)
    {
        _openingAmount = openingAmount;
        return this;
    }

    public CashSessionBuilder WithNotes(string? notes)
    {
        _notes = notes;
        return this;
    }

    public CashSession Build()
    {
        if (_establishmentId == Guid.Empty)
            throw new InvalidOperationException("No establishment id has been added");

        if (_attendantId == Guid.Empty)
            throw new InvalidOperationException("No attendant id has been added");

        return new CashSession(
            establishmentId: _establishmentId,
            attendantId: _attendantId,
            attendantName: _attendantName,
            openingAmount: _openingAmount,
            notes: _notes
        );
    }
}