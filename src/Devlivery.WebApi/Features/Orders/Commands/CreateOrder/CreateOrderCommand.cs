using FluentValidation;

namespace Devlivery.WebApi.Features.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    OrderItemDto[] Items,
    string CustomerName,
    string CustomerPhone,
    string DeliveryAddress,
    decimal DeliveryFee = 0);

public sealed record OrderItemDto(Guid ProductId, int Quantity, string? Notes);

public sealed class Validator : AbstractValidator<CreateOrderCommand>
{
    public Validator()
    {
        RuleFor(x => x.Items).NotEmpty().WithMessage("O campo '{PropertyName}' não pode estar vazio.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId).NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.");
            item.RuleFor(x => x.Quantity).GreaterThan(0)
                .WithMessage("O campo '{PropertyName}' deve ser maior que {ComparisonValue}.");
        });

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.")
            .MaximumLength(200).WithMessage("O campo '{PropertyName}' deve ter no máximo {MaxLength} caracteres.");

        RuleFor(x => x.CustomerPhone)
            .NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.")
            .MaximumLength(20).WithMessage("O campo '{PropertyName}' deve ter no máximo {MaxLength} caracteres.");

        RuleFor(x => x.DeliveryAddress)
            .NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.")
            .MaximumLength(500).WithMessage("O campo '{PropertyName}' deve ter no máximo {MaxLength} caracteres.");

        RuleFor(x => x.DeliveryFee)
            .GreaterThanOrEqualTo(0).WithMessage("O campo '{PropertyName}' deve ser maior ou igual a {ComparisonValue}.");
    }
}