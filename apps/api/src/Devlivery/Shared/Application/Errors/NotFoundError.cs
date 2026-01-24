namespace Devlivery.Shared.Application.Errors;

public sealed class NotFoundError(string message) : ErrorBase("Recurso não encontrado.", new[] { message })
{
}