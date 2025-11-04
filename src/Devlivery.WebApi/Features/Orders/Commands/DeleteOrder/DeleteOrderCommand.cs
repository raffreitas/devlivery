using FluentValidation;

namespace Devlivery.WebApi.Features.Orders.Commands.DeleteOrder;

public sealed record DeleteOrderCommand(Guid Id);

public sealed class Validator : AbstractValidator<DeleteOrderCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.");
    }
}
