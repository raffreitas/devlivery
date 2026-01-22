namespace Devlivery.Infrastructure.Identity.Authentication;

public interface ICurrentUserAccessor
{
    Guid UserId { get; }

    CurrentUser GetCurrentUser();
}

public sealed record CurrentUser(Guid Id, Guid EstablishmentId);