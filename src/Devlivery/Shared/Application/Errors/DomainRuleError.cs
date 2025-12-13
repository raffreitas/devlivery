namespace Devlivery.Shared.Application.Errors;

public sealed class DomainRuleError(string message)
    : ErrorBase("Um ou mais erros de regra de negócio ocorreram.", [message])
{
}