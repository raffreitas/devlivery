using Devlivery.Shared.Application.Errors;

using FluentResults;

using FluentValidation;

using Mediator;

namespace Devlivery.Shared.Application.Behaviors;

public sealed class ValidationPipelineBehavior<TRequest, TResponse>(
    ILogger<ValidationPipelineBehavior<TRequest, TResponse>> logger,
    IValidator<TRequest> validator
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    public ValueTask<TResponse> Handle(TRequest message, MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(message);
        if (validationResult.IsValid)
        {
            return next(message, cancellationToken);
        }

        logger.LogWarning("Validation failed for {RequestType}: {Errors}",
            typeof(TRequest).Name,
            string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
        TResponse result = (dynamic)Result.Fail(new ValidationError(errors));
        return ValueTask.FromResult(result);
    }
}