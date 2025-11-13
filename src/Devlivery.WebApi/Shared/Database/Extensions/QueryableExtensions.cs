using Microsoft.EntityFrameworkCore;

namespace Devlivery.WebApi.Shared.Database.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ForTenant<T>(this DbSet<T> dbSet, Guid tenantId) where T : class
        => dbSet.Where(e => EF.Property<Guid>(e, "EstablishmentId") == tenantId);
}