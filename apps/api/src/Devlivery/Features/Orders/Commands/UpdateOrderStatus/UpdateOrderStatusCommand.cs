using Devlivery.Domain.Aggregates.Orders.Enums;

using FluentValidation;

namespace Devlivery.Features.Orders.Commands.UpdateOrderStatus;

public sealed record UpdateOrderStatusCommand(Guid Id, OrderStatus Status);

public sealed class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithName("Pedido");
        RuleFor(x => x.Status).IsInEnum().WithName("Status");
    }
}