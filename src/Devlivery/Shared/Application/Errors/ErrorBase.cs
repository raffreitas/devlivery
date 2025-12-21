
using FluentResults;

namespace Devlivery.Shared.Application.Errors;

public abstract class ErrorBase(string message, string[] errors) : IError
{
    public List<IError> Reasons => [];

    public string Message => message;

    public Dictionary<string, object> Metadata => new() { { "Errors", errors } };
}
