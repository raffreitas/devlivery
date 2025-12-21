using System.Data.Common;

using Devlivery.Shared.Infrastructure.Persistence.Abstractions;

using Npgsql;

namespace Devlivery.Shared.Infrastructure.Persistence.Factory;

internal sealed class DbConnectionFactory(NpgsqlDataSource dataSource) : IDbConnectionFactory
{
    public async ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        return await dataSource.OpenConnectionAsync(cancellationToken);
    }
}