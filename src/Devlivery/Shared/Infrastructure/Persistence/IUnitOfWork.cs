using Microsoft.EntityFrameworkCore.Storage;

namespace Devlivery.Shared.Infrastructure.Persistence;

/// <summary>
/// Unit of Work interface for managing database transactions.
/// Ensures Domain Events are dispatched via the DispatchDomainEventsInterceptor.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all changes to the database.
    /// Domain Events are automatically dispatched by the DispatchDomainEventsInterceptor.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction.
    /// Use this for explicit transaction control when needed.
    /// </summary>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}