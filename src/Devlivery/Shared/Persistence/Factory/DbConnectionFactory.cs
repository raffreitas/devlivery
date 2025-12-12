using System.Data.Common;
using Devlivery.Shared.Persistence.Abstractions;
using Npgsql;

namespace Devlivery.Shared.Persistence.Factory;

internal sealed class DbConnectionFactory(NpgsqlDataSource dataSource) : IDbConnectionFactory
{
    public async ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        return await dataSource.OpenConnectionAsync(cancellationToken);
    }
}