using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Devlivery.Common.Errors;
using Devlivery.Infrastructure.Persistence.Context;
using Devlivery.Infrastructure.Tenancy;

using FluentResults;

using Microsoft.EntityFrameworkCore;

using Npgsql;

namespace Devlivery.Infrastructure.Persistence.Idempotency;

public sealed record IdempotencyKey(string Operation, Guid ScopeId, Guid RequestId);
public sealed record IdempotentResult<T>(T Response, bool Replayed);

public sealed class IdempotencyStore(ApplicationDbContext db, ITenantAccessor tenants, IUnitOfWork unitOfWork)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // The callback stages its writes in the current unit of work. This method commits both together.
    public async Task<Result<IdempotentResult<T>>> ExecuteAsync<T>(
        IdempotencyKey key,
        Guid actorId,
        string canonicalRequest,
        Func<CancellationToken, Task<Result<T>>> execute,
        CancellationToken ct) where T : notnull
    {
        var requestHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalRequest)));
        var existing = await FindAsync(key, ct);
        if (existing is not null)
            return Replay<T>(existing, actorId, requestHash);

        var result = await execute(ct);
        if (result.IsFailed)
        {
            // A competing request may have changed the resource after the first lookup.
            existing = await FindAsync(key, ct);
            return existing is null ? Result.Fail(result.Errors) : Replay<T>(existing, actorId, requestHash);
        }

        db.Add(new IdempotencyRecord
        {
            EstablishmentId = tenants.Tenant.Id,
            Operation = key.Operation,
            ScopeId = key.ScopeId,
            RequestId = key.RequestId,
            ActorId = actorId,
            RequestHash = requestHash,
            ResponseJson = JsonSerializer.Serialize(result.Value, JsonOptions)
        });

        try
        {
            await unitOfWork.CommitAsync(ct);
        }
        catch (DbUpdateException ex) when (ex is DbUpdateConcurrencyException || ex.InnerException is PostgresException
               {
                   SqlState: PostgresErrorCodes.UniqueViolation,
                   ConstraintName: IdempotencyRecordConfiguration.KeyName
               })
        {
            // SaveChanges rolled back. Look for the winner in committed state.
            existing = await FindAsync(key, ct);
            return existing is null
                ? Result.Fail(new ConflictError("O registro foi atualizado. Confira os dados e tente novamente."))
                : Replay<T>(existing, actorId, requestHash);
        }

        return Result.Ok(new IdempotentResult<T>(result.Value, Replayed: false));
    }

    private Task<IdempotencyRecord?> FindAsync(IdempotencyKey key, CancellationToken ct) =>
        db.Set<IdempotencyRecord>().AsNoTracking().FirstOrDefaultAsync(
            x => x.Operation == key.Operation && x.ScopeId == key.ScopeId && x.RequestId == key.RequestId, ct);

    private static Result<IdempotentResult<T>> Replay<T>(IdempotencyRecord record, Guid actorId, string requestHash)
        where T : notnull =>
        record.ActorId == actorId && record.RequestHash == requestHash
            ? Result.Ok(new IdempotentResult<T>(JsonSerializer.Deserialize<T>(record.ResponseJson, JsonOptions)!, Replayed: true))
            : Result.Fail(new ConflictError("Este identificador já foi usado em outra requisição."));
}
