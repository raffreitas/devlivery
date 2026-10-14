using Devlivery.Domain.Common.Enums;

using FluentValidation;

namespace Devlivery.Features.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    OrderItemDto[] Items,
    string CustomerName,
    string? CustomerPhone,
    string DeliveryAddress,
    OrderPaymentDto[] Payments,
    decimal DeliveryFee = 0,
    string? DeliveryReference = null,
    string? Notes = null);

public sealed record OrderItemDto(Guid ProductId, int Quantity, string? Notes);
public sealed record OrderPaymentDto(PaymentMethod Method, decimal Amount);

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty().WithName("Itens");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId).NotEmpty().WithName("Produto");
            item.RuleFor(x => x.Quantity).GreaterThan(0).WithName("Quantidade");
        });

        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200).WithName("Nome do cliente");
        RuleFor(x => x.CustomerPhone).MaximumLength(20).WithName("Telefone do cliente");

        RuleFor(x => x.Payments).NotEmpty().WithName("Pagamentos");
        RuleForEach(x => x.Payments).ChildRules(payment =>
        {
            payment.RuleFor(p => p.Method).IsInEnum().WithName("Forma de pagamento");
            payment.RuleFor(p => p.Amount).GreaterThan(0).WithName("Valor do pagamento");
        });

        RuleFor(x => x.DeliveryAddress).NotEmpty().MaximumLength(500).WithName("Endereço de entrega");
        RuleFor(x => x.DeliveryFee).GreaterThanOrEqualTo(0).WithName("Taxa de entrega");
        RuleFor(x => x.Notes).MaximumLength(500).WithName("Observações");
    }
}