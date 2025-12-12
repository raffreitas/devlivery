using FluentResults;

namespace Devlivery.Shared.SeedWork.Errors;

public sealed class BusinessRuleError(string message) : Error(message)
{
}