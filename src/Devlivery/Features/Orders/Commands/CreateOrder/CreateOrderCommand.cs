using Devlivery.Features.Orders.Domain.Enums;

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
                .MaximumLength(20).WithMessage("O campo '{PropertyName}' deve ter no máximo {MaxLength} caracteres.")
                .Must(phone => IsValidBrazilianPhone(phone!))
                .WithMessage(
                    "O campo '{PropertyName}' deve ser um telefone válido (formato: (XX) XXXXX-XXXX ou (XX) XXXX-XXXX).");
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

    private static bool IsValidBrazilianPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        // Remove formatting characters
        var cleaned = new string(phone.Where(char.IsDigit).ToArray());

        // Brazilian phone numbers: 10 digits (landline) or 11 digits (mobile with area code)
        // Format: (XX) XXXXX-XXXX (11 digits) or (XX) XXXX-XXXX (10 digits)
        if (cleaned.Length is < 10 or > 11)
            return false;

        // First two digits should be area code (11-99 for valid Brazilian area codes)
        if (cleaned.Length >= 2)
        {
            if (!int.TryParse(cleaned[..2], out var areaCode))
                return false;

            if (areaCode is < 11 or > 99)
                return false;
        }

        return true;
    }
}