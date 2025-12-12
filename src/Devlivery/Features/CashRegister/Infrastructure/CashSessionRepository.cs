using Devlivery.Features.CashRegister.Domain;
using Devlivery.Shared.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.CashRegister.Infrastructure;

/// <summary>
/// Repository for CashSession aggregate.
/// Handles write operations and complex queries for Cash Sessions.
/// </summary>
public sealed class CashSessionRepository(ApplicationDbContext dbContext)
{
    /// <summary>
    /// Gets a cash session by ID, including deposits.
    /// </summary>
    public async Task<CashSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.CashSessions
            .Include(cs => cs.Deposits)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct);
    }

    /// <summary>
    /// Gets the currently active (open) cash session.
    /// </summary>
    public async Task<CashSession?> GetActiveSessionAsync(CancellationToken ct = default)
    {
        return await dbContext.CashSessions
            .Where(cs => cs.Status == CashSessionStatus.Open)
            .OrderByDescending(cs => cs.StartAt)
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Adds a new cash session to the database.
    /// </summary>
    public async Task AddAsync(CashSession session, CancellationToken ct = default)
    {
        await dbContext.CashSessions.AddAsync(session, ct);
    }

    /// <summary>
    /// Updates an existing cash session.
    /// </summary>
    public void Update(CashSession session)
    {
        dbContext.CashSessions.Update(session);
    }
}
