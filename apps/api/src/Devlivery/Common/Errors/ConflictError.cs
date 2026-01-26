using FluentResults;

namespace Devlivery.Common.Errors;

public sealed class ConflictError(string message) : Error(message)
{
    public const string Name = nameof(ConflictError);
};