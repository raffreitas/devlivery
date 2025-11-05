using FluentValidation;

namespace Devlivery.WebApi.Features.Orders.Commands.UpdateOrderStatus;

public sealed record UpdateOrderStatusCommand(Guid Id, string Status);

public sealed class Validator : AbstractValidator<UpdateOrderStatusCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.");
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(status => new[] { "pending", "preparing", "ready", "delivered", "cancelled" }.Contains(status))
            .WithMessage("Status deve ser: pending, preparing, ready, delivered ou cancelled");
    }
}
