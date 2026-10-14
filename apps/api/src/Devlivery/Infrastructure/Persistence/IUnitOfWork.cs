namespace Devlivery.Infrastructure.Persistence;

public interface IUnitOfWork
{
    /// <summary>
    /// Dispatches pending domain events (including events raised by their handlers)
    /// and then persists every tracked change in a single save.
    /// </summary>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}