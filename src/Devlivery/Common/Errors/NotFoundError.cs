using FluentResults;

namespace Devlivery.Common.Errors;

public sealed class NotFoundError(string message) : Error(message)
{
    public const string Name = nameof(NotFoundError);
};