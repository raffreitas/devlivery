using FluentResults;

namespace Devlivery.WebApi.Shared.Errors;

public sealed class BusinessRuleError(string message) : Error(message)
{
}