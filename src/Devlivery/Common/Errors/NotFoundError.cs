namespace Devlivery.Common.Errors;

public sealed class NotFoundError(string message) : ErrorBase("Recurso não encontrado.", new[] { message })
{
}