using Devlivery.Shared.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace Devlivery.Shared.Infrastructure.Persistence;

/// <summary>
/// Encapsulates ApplicationDbContext and manages transactions.
/// Ensures Domain Events are dispatched via the DispatchDomainEventsInterceptor.
/// </summary>
public sealed class UnitOfWork(ApplicationDbContext dbContext)
{
    /// <summary>
    /// Saves all changes to the database.
    /// Domain Events are automatically dispatched by the DispatchDomainEventsInterceptor.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Begins a new database transaction.
    /// Use this for explicit transaction control when needed.
    /// </summary>
    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Database.BeginTransactionAsync(cancellationToken);
    }
}
