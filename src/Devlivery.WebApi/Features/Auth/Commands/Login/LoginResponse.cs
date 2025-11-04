namespace Devlivery.WebApi.Features.Auth.Commands.Login;

public sealed record LoginResponse(Guid UserId, string UserName, string Token);