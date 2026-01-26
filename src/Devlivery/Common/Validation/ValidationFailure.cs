namespace Devlivery.Common.Validation;

public sealed record ValidationFailure(string Field, string Message);