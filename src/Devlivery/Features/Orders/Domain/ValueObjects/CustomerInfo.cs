using Devlivery.Shared.SeedWork;

namespace Devlivery.Features.Orders.Domain.ValueObjects;

public sealed record CustomerInfo
{
    public string Name { get; private init; } = null!;
    public PhoneNumber? Phone { get; private init; }

    private CustomerInfo()
    {
    }

    private CustomerInfo(string name, PhoneNumber? phone = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome do cliente é obrigatório", nameof(name));

        if (name.Length < 3)
            throw new ArgumentException("Nome do cliente deve ter pelo menos 3 caracteres", nameof(name));

        Name = name.Trim();
        Phone = phone;
    }

    public static CustomerInfo Create(string name, string? phoneNumber = null)
    {
        PhoneNumber? phone = null;
        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            phone = new PhoneNumber(phoneNumber);
        }

        return new CustomerInfo(name, phone);
    }

    public override string ToString() => Phone != null ? $"{Name} ({Phone})" : Name;
}