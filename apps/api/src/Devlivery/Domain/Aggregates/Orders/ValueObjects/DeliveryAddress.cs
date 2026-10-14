using Devlivery.Domain.SeedWork;

namespace Devlivery.Domain.Aggregates.Orders.ValueObjects;

public sealed record DeliveryAddress
{
    public string FullAddress { get; private init; }
    public string? Reference { get; private init; }

    public DeliveryAddress(string fullAddress, string? reference = null)
    {
        if (string.IsNullOrWhiteSpace(fullAddress))
            throw new DomainException("Endereço de entrega é obrigatório");

        FullAddress = fullAddress.Trim();
        Reference = reference?.Trim();
    }

    public override string ToString() => FullAddress;

    public static implicit operator string(DeliveryAddress address) => address.FullAddress;
}