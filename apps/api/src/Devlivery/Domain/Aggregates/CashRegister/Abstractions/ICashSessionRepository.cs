namespace Devlivery.Domain.Aggregates.CashRegister.Abstractions;

/// <summary>
/// Cash sessions returned by this repository are tracked: changes are persisted by <c>IUnitOfWork.SaveChangesAsync</c>.
/// </summary>
public interface ICashSessionRepository
{
    Task<CashSession?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<CashSession?> GetActiveSessionAsync(CancellationToken ct = default);

    Task AddAsync(CashSession session, CancellationToken ct = default);
}