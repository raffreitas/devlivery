using Devlivery.Common.Errors;
using Devlivery.Common.Handlers;

using FluentResults;

using FluentValidation;

namespace Devlivery.Common.Validation;

/// <summary>
/// Runs every <see cref="IValidator{T}"/> registered for the command before the decorated handler.
/// An invalid command returns a <see cref="ValidationError"/> without reaching the handler.
/// </summary>
public sealed class ValidationCommandHandlerDecorator<TCommand, TResponse>(
    ICommandHandler<TCommand, TResponse> inner,
    IEnumerable<IValidator<TCommand>> validators)
    : ICommandHandler<TCommand, TResponse>
    where TResponse : ResultBase<TResponse>, new()
{
    public async Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken)
    {
        var failures = new List<ValidationFailure>();

        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(command, cancellationToken);
            failures.AddRange(result.Errors.Select(e => new ValidationFailure(e.PropertyName, e.ErrorMessage)));
        }

        if (failures.Count != 0)
            return new TResponse().WithError(new ValidationError([.. failures]));

        return await inner.HandleAsync(command, cancellationToken);
    }
}