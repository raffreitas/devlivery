using System.Data.Common;

using Devlivery.Infrastructure.Persistence.Abstractions;

using Npgsql;

namespace Devlivery.Infrastructure.Persistence.Factory;

internal sealed class DbConnectionFactory(NpgsqlDataSource dataSource) : IDbConnectionFactory
{
    public async ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        return await dataSource.OpenConnectionAsync(cancellationToken);
    }
}