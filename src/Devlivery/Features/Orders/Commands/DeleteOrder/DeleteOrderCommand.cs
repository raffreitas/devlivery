using Devlivery.Shared.Extensions;

using FluentResults;

using FluentValidation;

using Mediator;

namespace Devlivery.Features.Orders.Commands.DeleteOrder;

public sealed record DeleteOrderCommand(Guid Id) : ICommand<Result>
{
    public bool IsValid(out string[] errors)
    {
        var result = new DeleteOrderCommandValidator().Validate(this);
        errors = result.GetErrors();
        return result.IsValid;
    }
};

public sealed class DeleteOrderCommandValidator : AbstractValidator<DeleteOrderCommand>
{
    public DeleteOrderCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.");
    }
}