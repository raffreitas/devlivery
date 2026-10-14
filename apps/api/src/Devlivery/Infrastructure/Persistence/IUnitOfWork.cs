namespace Devlivery.Infrastructure.Persistence;

/// <summary>
/// Unit of Work for persisting the changes of a request.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Dispatches pending domain events (including events raised by their handlers)
    /// and then persists every tracked change in a single save.
    /// </summary>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}