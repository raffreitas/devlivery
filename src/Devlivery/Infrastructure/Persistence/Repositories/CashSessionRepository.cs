using Devlivery.Domain.Aggregates.CashRegister;
using Devlivery.Domain.Aggregates.CashRegister.Abstractions;
using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Infrastructure.Persistence.Context;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for CashSession aggregate.
/// Handles write operations and complex queries for Cash Sessions.
/// </summary>
public sealed class CashSessionRepository(ApplicationDbContext dbContext) : ICashSessionRepository
{
    /// <summary>
    /// Gets a cash session by ID, including deposits.
    /// </summary>
    public async Task<CashSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.CashSessions
            .Include(cs => cs.Movements)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct);
    }

    /// <summary>
    /// Gets the currently active (open) cash session.
    /// </summary>
    public async Task<CashSession?> GetActiveSessionAsync(CancellationToken ct = default)
    {
        return await dbContext.CashSessions
            .Include(x => x.Movements)
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
    public Task UpdateAsync(CashSession session, CancellationToken ct = default)
    {
        dbContext.CashSessions.Update(session);
        return Task.CompletedTask;
    }
}