using FluentResults;

namespace Devlivery.Shared.SeedWork.Errors;

public sealed class NotFoundError(string message) : Error(message)
{
}