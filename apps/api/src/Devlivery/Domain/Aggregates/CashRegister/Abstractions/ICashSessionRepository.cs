namespace Devlivery.Domain.Aggregates.CashRegister.Abstractions;

public interface ICashSessionRepository
{
    Task<CashSession?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<CashSession?> GetActiveSessionAsync(CancellationToken ct = default);

    Task AddAsync(CashSession session, CancellationToken ct = default);

    Task UpdateAsync(CashSession session, CancellationToken ct = default);
}