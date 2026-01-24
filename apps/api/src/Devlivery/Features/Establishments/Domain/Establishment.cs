using Devlivery.Shared.SeedWork;

namespace Devlivery.Features.Establishments.Domain;

public sealed class Establishment : Entity
{
    public string TradeName { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Establishment(string tradeName, bool isActive)
    {
        TradeName = tradeName;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string? tradeName = null)
    {
        if (!string.IsNullOrWhiteSpace(tradeName))
            TradeName = tradeName;

        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}