using Devlivery.Shared.Application.Errors;

namespace Devlivery.Features.Auth.Shared;

public static class AuthErrors
{
    public static UnauthorizedError InvalidCredentials =>
        new("Credenciais inválidas");
}
