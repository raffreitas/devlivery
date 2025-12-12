using System.Data.Common;

namespace Devlivery.Shared.Infrastructure.Persistence.Abstractions;

public interface IDbConnectionFactory
{
    ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}