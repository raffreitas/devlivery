using FluentResults;

namespace Devlivery.Common.Errors;

public sealed class ForbiddenError(string message) : Error(message)
{
    public const string Name = nameof(ForbiddenError);
};