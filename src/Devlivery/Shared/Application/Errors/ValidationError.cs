namespace Devlivery.Shared.Application.Errors;

public class ValidationError(string[] errors) : ErrorBase("Um ou mais erros de validação ocorreram", errors)
{
}