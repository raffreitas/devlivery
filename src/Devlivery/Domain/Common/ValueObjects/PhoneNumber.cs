using Devlivery.Domain.SeedWork;

namespace Devlivery.Domain.Common.ValueObjects;

/// <summary>
/// Value Object representing a phone number with basic validation.
/// </summary>
public sealed record PhoneNumber
{
    public string Number { get; init; }

    public PhoneNumber(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new DomainException("Número de telefone não pode ser vazio");

        // Remove formatting characters
        var cleaned = new string([.. number.Where(char.IsDigit)]);

        if (cleaned.Length is < 10 or > 11)
            throw new DomainException("Número de telefone deve ter entre 10 e 11 dígitos");

        Number = cleaned;
    }

    /// <summary>
    /// Returns formatted phone number: (XX) XXXXX-XXXX or (XX) XXXX-XXXX
    /// </summary>
    private string Formatted
    {
        get
        {
            return Number.Length == 11
                ? $"({Number[..2]}) {Number[2..7]}-{Number[7..]}"
                : $"({Number[..2]}) {Number[2..6]}-{Number[6..]}";
        }
    }

    public override string ToString() => Formatted;

    public static implicit operator string?(PhoneNumber? phone) => phone?.Number;
}