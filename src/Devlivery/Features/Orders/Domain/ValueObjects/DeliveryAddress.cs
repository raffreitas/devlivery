namespace Devlivery.Features.Orders.Domain.ValueObjects;

/// <summary>
/// Value Object representing a delivery address.
/// Stores the complete address as a single string (as provided by frontend).
/// </summary>
public sealed record DeliveryAddress
{
    public string FullAddress { get; private init; }
    public string? Reference { get; private init; }

    public DeliveryAddress(string fullAddress, string? reference = null)
    {
        if (string.IsNullOrWhiteSpace(fullAddress))
            throw new ArgumentException("Endereço de entrega é obrigatório", nameof(fullAddress));

        if (fullAddress.Length < 10)
            throw new ArgumentException("Endereço de entrega deve ser mais detalhado", nameof(fullAddress));

        FullAddress = fullAddress.Trim();
        Reference = reference?.Trim();
    }

    /// <summary>
    /// Returns the address with reference for delivery driver.
    /// </summary>
    public string DeliveryInstructions => Reference != null
        ? $"{FullAddress}\nReferência: {Reference}"
        : FullAddress;

    public override string ToString() => FullAddress;

    public static implicit operator string(DeliveryAddress address) => address.FullAddress;
}