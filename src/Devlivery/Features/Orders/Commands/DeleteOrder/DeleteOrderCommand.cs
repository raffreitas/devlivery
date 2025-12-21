using FluentResults;

using FluentValidation;

using Mediator;

namespace Devlivery.Features.Orders.Commands.DeleteOrder;

public sealed record DeleteOrderCommand(Guid Id) : ICommand<Result>;

public sealed class DeleteOrderCommandValidator : AbstractValidator<DeleteOrderCommand>
{
    public DeleteOrderCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.");
    }
}