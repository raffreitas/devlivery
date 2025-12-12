using FluentResults;

namespace Devlivery.Shared.Errors;

public sealed class NotFoundError(string message) : Error(message)
{
}