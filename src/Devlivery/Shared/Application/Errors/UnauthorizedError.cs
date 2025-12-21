namespace Devlivery.Shared.Application.Errors;

public sealed class UnauthorizedError(string message)
    : ErrorBase("Acesso não autorizado.", [message])
{
}
