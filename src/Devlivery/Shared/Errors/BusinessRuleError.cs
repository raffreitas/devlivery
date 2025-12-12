using FluentResults;

namespace Devlivery.Shared.Errors;

public sealed class BusinessRuleError(string message) : Error(message)
{
}