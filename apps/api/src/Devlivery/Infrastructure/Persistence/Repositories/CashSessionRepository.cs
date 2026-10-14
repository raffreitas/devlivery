using Devlivery.Domain.Aggregates.CashRegister;
using Devlivery.Domain.Aggregates.CashRegister.Abstractions;
using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Infrastructure.Persistence.Context;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Infrastructure.Persistence.Repositories;

public sealed class CashSessionRepository(ApplicationDbContext dbContext) : ICashSessionRepository
{
    public Task<CashSession?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        dbContext.CashSessions
            .Include(cs => cs.Movements)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct);

    public Task<CashSession?> GetActiveSessionAsync(CancellationToken ct = default) =>
        dbContext.CashSessions
            .Include(cs => cs.Movements)
            .Where(cs => cs.Status == CashSessionStatus.Open)
            .OrderByDescending(cs => cs.StartAt)
            .FirstOrDefaultAsync(ct);

    public async Task AddAsync(CashSession session, CancellationToken ct = default) =>
        await dbContext.CashSessions.AddAsync(session, ct);
}