using Devlivery.Features.Orders.Domain;

using FluentResults;

using FluentValidation;

using Mediator;

namespace Devlivery.Features.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    OrderItemDto[] Items,
    string CustomerName,
    string? CustomerPhone,
    string DeliveryAddress,
    PaymentMethod PaymentMethod,
    decimal DeliveryFee = 0,
    string? DeliveryReference = null,
    string? Notes = null) : ICommand<Result<CreateOrderResponse>>;

public sealed record OrderItemDto(Guid ProductId, int Quantity, string? Notes);

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
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

        When(x => !string.IsNullOrWhiteSpace(x.CustomerPhone), () =>
        {
            RuleFor(x => x.CustomerPhone)
                .MaximumLength(20).WithMessage("O campo '{PropertyName}' deve ter no máximo {MaxLength} caracteres.");
        });

        RuleFor(x => x.PaymentMethod)
            .NotNull().WithMessage("O campo '{PropertyName}' é obrigatório.")
            .IsInEnum().WithMessage("O campo '{PropertyName}' deve ser um método de pagamento válido.");

        RuleFor(x => x.DeliveryAddress)
            .NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.")
            .MaximumLength(500).WithMessage("O campo '{PropertyName}' deve ter no máximo {MaxLength} caracteres.");

        RuleFor(x => x.DeliveryFee)
            .GreaterThanOrEqualTo(0)
            .WithMessage("O campo '{PropertyName}' deve ser maior ou igual a {ComparisonValue}.");

        When(x => !string.IsNullOrWhiteSpace(x.Notes), () =>
        {
            RuleFor(x => x.Notes).MaximumLength(500)
                .WithMessage("O campo '{PropertyName}' deve ter no máximo {MaxLength} caracteres.");
        });
    }
}