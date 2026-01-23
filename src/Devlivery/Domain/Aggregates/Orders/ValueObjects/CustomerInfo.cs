using Devlivery.Domain.Common.ValueObjects;
using Devlivery.Domain.SeedWork;

namespace Devlivery.Domain.Aggregates.Orders.ValueObjects;

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
            throw new DomainException("Nome do cliente é obrigatório");

        if (name.Length < 3)
            throw new DomainException("Nome do cliente deve ter pelo menos 3 caracteres");

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