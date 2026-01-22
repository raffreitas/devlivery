using FluentResults;

namespace Devlivery.Common.Errors;

public sealed class UnauthorizedError(string message) : Error(message)
{
    public const string Name = nameof(UnauthorizedError);
};