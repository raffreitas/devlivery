namespace Devlivery.Shared.Application.Errors;

public sealed class ForbiddenError() : ErrorBase("Acesso negado", ["Você não tem permissão para acessar este recurso."])
{
}