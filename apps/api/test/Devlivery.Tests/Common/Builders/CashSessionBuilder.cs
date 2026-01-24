using Bogus;

using Devlivery.Features.CashRegister.Domain;
using Devlivery.Features.CashRegister.Domain.Enums;
using Devlivery.Shared.Domain.Enums;

namespace Devlivery.Tests.Common.Builders;

public sealed class CashSessionBuilder
{
    private readonly Faker _faker = new();

    private Guid _establishmentId;
    private Guid _attendantId;
    private string _attendantName;
    private decimal _openingAmount;
    private string? _notes;
    private readonly List<MovementConfig> _movements = [];
    private bool _shouldClose;
    private decimal? _closingAmount;

    private sealed record MovementConfig(
        CashSessionEntryType EntryType,
        decimal Amount,
        PaymentMethod? PaymentMethod,
        Guid? OrderId,
        Guid? OrderPaymentId,
        string? Reason);

    public CashSessionBuilder()
    {
        _establishmentId = Guid.NewGuid();
        _attendantId = Guid.NewGuid();
        _attendantName = _faker.Name.FullName();
        _openingAmount = _faker.Random.Decimal(0, 1000);
        _notes = null;
        _shouldClose = false;
        _closingAmount = null;
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

    public CashSessionBuilder WithPayment(decimal amount, PaymentMethod paymentMethod, Guid? orderId = null, Guid? orderPaymentId = null)
    {
        _movements.Add(new MovementConfig(
            CashSessionEntryType.Payment,
            amount,
            paymentMethod,
            orderId ?? Guid.NewGuid(),
            orderPaymentId ?? Guid.NewGuid(),
            null));
        return this;
    }

    public CashSessionBuilder WithDeposit(decimal amount, string? reason = null)
    {
        _movements.Add(new MovementConfig(
            CashSessionEntryType.Deposit,
            amount,
            null,
            null,
            null,
            reason));
        return this;
    }

    public CashSessionBuilder WithReversal(decimal amount, PaymentMethod paymentMethod, Guid orderPaymentId, Guid? orderId = null, string? reason = null)
    {
        _movements.Add(new MovementConfig(
            CashSessionEntryType.Refund,
            amount,
            paymentMethod,
            orderId ?? Guid.NewGuid(),
            orderPaymentId,
            reason ?? "Pedido cancelado"));
        return this;
    }

    public CashSessionBuilder WithChange(decimal amount, Guid? orderId = null, PaymentMethod paymentMethod = PaymentMethod.Cash)
    {
        _movements.Add(new MovementConfig(
            CashSessionEntryType.Change,
            amount,
            paymentMethod,
            orderId ?? Guid.NewGuid(),
            null,
            null));
        return this;
    }

    public CashSessionBuilder WithMultiplePayments(params (decimal Amount, PaymentMethod Method)[] payments)
    {
        foreach (var (amount, method) in payments)
        {
            WithPayment(amount, method);
        }
        return this;
    }

    public CashSessionBuilder AsClosed(decimal? closingAmount = null)
    {
        _shouldClose = true;
        _closingAmount = closingAmount;
        return this;
    }

    public CashSession Build()
    {
        if (_establishmentId == Guid.Empty)
            throw new InvalidOperationException("No establishment id has been added");

        if (_attendantId == Guid.Empty)
            throw new InvalidOperationException("No attendant id has been added");

        var cashSession = new CashSession(
            establishmentId: _establishmentId,
            attendantId: _attendantId,
            attendantName: _attendantName,
            openingAmount: _openingAmount,
            notes: _notes
        );

        // Apply movements
        foreach (var movement in _movements)
        {
            switch (movement.EntryType)
            {
                case CashSessionEntryType.Payment:
                    cashSession.AddPayment(
                        movement.OrderPaymentId!.Value,
                        movement.Amount,
                        movement.PaymentMethod!.Value,
                        movement.OrderId!.Value);
                    break;

                case CashSessionEntryType.Deposit:
                    cashSession.AddDeposit(
                        movement.Amount,
                        _attendantId,
                        movement.Reason);
                    break;

                case CashSessionEntryType.Refund:
                    cashSession.AddReversal(
                        movement.OrderPaymentId!.Value,
                        movement.Amount,
                        movement.PaymentMethod!.Value,
                        movement.Reason ?? "Pedido cancelado",
                        movement.OrderId!.Value);
                    break;

                case CashSessionEntryType.Change:
                    cashSession.AddChange(
                        movement.OrderId!.Value,
                        movement.Amount,
                        movement.PaymentMethod ?? PaymentMethod.Cash);
                    break;
            }
        }

        // Close if requested
        if (_shouldClose)
        {
            cashSession.Close(_closingAmount ?? cashSession.ExpectedCashAmount, null);
        }

        return cashSession;
    }
}