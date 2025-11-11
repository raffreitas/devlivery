using System.Data.Common;
using Devlivery.WebApi.Shared.Database.Abstractions;
using Npgsql;

namespace Devlivery.WebApi.Shared.Database.Factory;

internal sealed class DbConnectionFactory(NpgsqlDataSource dataSource) : IDbConnectionFactory
{
    public async ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        return await dataSource.OpenConnectionAsync(cancellationToken);
    }
}