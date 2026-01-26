namespace Devlivery.Common.Validation;

public sealed record Validation(bool IsFailure, ValidationFailure[] Errors);