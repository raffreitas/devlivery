using FluentResults;

namespace Devlivery.WebApi.Shared.Errors;

public sealed class NotFoundError(string message) : Error(message)
{
}