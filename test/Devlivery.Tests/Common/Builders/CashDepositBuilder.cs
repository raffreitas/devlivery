using Bogus;

using Devlivery.Features.CashRegister.Domain.Entities;

namespace Devlivery.Tests.Common.Builders;

public sealed class CashDepositBuilder
{
    private readonly Faker _faker = new();

    private Guid _cashSessionId;
    private Guid _establishmentId;
    private Guid _attendantId;
    private string _attendantName;
    private decimal _amount;
    private string? _notes;

    public CashDepositBuilder()
    {
        _cashSessionId = Guid.NewGuid();
        _establishmentId = Guid.NewGuid();
        _attendantId = Guid.NewGuid();
        _attendantName = _faker.Name.FullName();
        _amount = _faker.Random.Decimal(10, 500);
        _notes = null;
    }

    public CashDepositBuilder WithCashSessionId(Guid cashSessionId)
    {
        _cashSessionId = cashSessionId;
        return this;
    }

    public CashDepositBuilder WithEstablishmentId(Guid establishmentId)
    {
        _establishmentId = establishmentId;
        return this;
    }

    public CashDepositBuilder WithAttendantId(Guid attendantId)
    {
        _attendantId = attendantId;
        return this;
    }

    public CashDepositBuilder WithAttendantName(string attendantName)
    {
        _attendantName = attendantName;
        return this;
    }

    public CashDepositBuilder WithAmount(decimal amount)
    {
        _amount = amount;
        return this;
    }

    public CashDepositBuilder WithNotes(string? notes)
    {
        _notes = notes;
        return this;
    }

    public CashDeposit Build()
    {
        if (_cashSessionId == Guid.Empty)
            throw new InvalidOperationException("No cash session id has been added");

        if (_establishmentId == Guid.Empty)
            throw new InvalidOperationException("No establishment id has been added");

        if (_attendantId == Guid.Empty)
            throw new InvalidOperationException("No attendant id has been added");

        return new CashDeposit(
            cashSessionId: _cashSessionId,
            establishmentId: _establishmentId,
            attendantId: _attendantId,
            attendantName: _attendantName,
            amount: _amount,
            notes: _notes
        );
    }
}