using System.Data.Common;

namespace Devlivery.Infrastructure.Persistence.Abstractions;

public interface IDbConnectionFactory
{
    ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}