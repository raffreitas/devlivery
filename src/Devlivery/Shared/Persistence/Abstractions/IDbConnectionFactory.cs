using System.Data.Common;

namespace Devlivery.Shared.Persistence.Abstractions;

public interface IDbConnectionFactory
{
    ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}