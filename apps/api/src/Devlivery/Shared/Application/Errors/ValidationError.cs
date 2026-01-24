namespace Devlivery.Shared.Application.Errors;

public sealed class ValidationError : ErrorBase
{
    private const string DefaultMessage = "Um ou mais erros de validação ocorreram";

    public ValidationError(string[] errors) : base(DefaultMessage, errors)
    {
    }

    public ValidationError(string error) : base(DefaultMessage, [error])
    {
    }
}