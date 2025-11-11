using System.Data.Common;

namespace Devlivery.WebApi.Shared.Database.Abstractions;

public interface IDbConnectionFactory
{
    ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}