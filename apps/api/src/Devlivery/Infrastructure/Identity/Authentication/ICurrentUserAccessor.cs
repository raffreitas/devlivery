namespace Devlivery.Infrastructure.Identity.Authentication;

public interface ICurrentUserAccessor
{
    Task<ResolvedCurrentUser> ResolveAsync(CancellationToken cancellationToken = default);

    Guid UserId { get; }

    CurrentUser GetCurrentUser();
}

public sealed record CurrentUser(Guid Id, Guid EstablishmentId);
public sealed record ResolvedCurrentUser(Guid Id, string Name);
