using Devlivery.Features.CashRegister.Domain;

namespace Devlivery.Features.CashRegister.Infrastructure;

/// <summary>
/// Repository interface for CashSession aggregate.
/// Provides abstraction for cash session persistence operations.
/// </summary>
public interface ICashSessionRepository
{
    /// <summary>
    /// Gets a cash session by ID, including deposits.
    /// </summary>
    Task<CashSession?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets the currently active (open) cash session.
    /// </summary>
    Task<CashSession?> GetActiveSessionAsync(CancellationToken ct = default);

    /// <summary>
    /// Adds a new cash session to the database.
    /// </summary>
    Task AddAsync(CashSession session, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing cash session.
    /// </summary>
    void Update(CashSession session);
}