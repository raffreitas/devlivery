using Devlivery.Features.Orders.Domain;
using Devlivery.Shared.Extensions;

using FluentResults;

using FluentValidation;

using Mediator;

namespace Devlivery.Features.Orders.Commands.UpdateOrderStatus;

public sealed record UpdateOrderStatusCommand(Guid Id, string Status) : ICommand<Result>
{
    public bool IsValid(out string[] errors)
    {
        var result = new UpdateOrderStatusCommandValidator().Validate(this);
        errors = result.GetErrors();
        return result.IsValid;
    }
};

public sealed class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.");
        RuleFor(x => x.Status)
            .NotEmpty()
            .IsEnumName(typeof(OrderStatus), caseSensitive: false)
            .WithMessage("Status inválido para o pedido.");
    }
}