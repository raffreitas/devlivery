using Devlivery.Domain.SeedWork;

namespace Devlivery.Domain.Common.ValueObjects;

/// <summary>
/// Value Object representing money in the system.
/// Encapsulates amount and ensures immutability and validation.
/// Currency is fixed to BRL (Brazilian Real) as the system operates only in Brazil.
/// </summary>
public sealed record Money
{
    public decimal Amount { get; init; }

    public Money(decimal amount)
    {
        if (amount < 0)
            throw new DomainException("Valor não pode ser negativo");

        Amount = amount;
    }

    private Money Add(Money other) => new(Amount + other.Amount);

    private Money Subtract(Money other) => new(Amount - other.Amount);

    public bool IsGreaterThan(Money other) => Amount > other.Amount;

    public bool IsLessThan(Money other) => Amount < other.Amount;

    public static Money Zero => new(0);

    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);

    public override string ToString() => $"R$ {Amount:N2}";

    /// <summary>
    /// Converts Money to decimal (Amount).
    /// </summary>
    public static implicit operator decimal(Money money) => money.Amount;
}