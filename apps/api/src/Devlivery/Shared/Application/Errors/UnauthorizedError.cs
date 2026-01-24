namespace Devlivery.Shared.Application.Errors;

public sealed class UnauthorizedError() : ErrorBase("Acesso não autorizado.", new[] { "Você não está autorizado a acessar este recurso." })
{
}