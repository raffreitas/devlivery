using Devlivery.Domain.Common.Enums;
using Devlivery.Features.Orders.Commands.CreateOrder;

using Shouldly;

namespace Devlivery.Tests.Features.Orders.Commands.CreateOrder;

[Trait("Category", "Unit Tests")]
public sealed class CreateOrderCommandValidatorTests
{
    [Fact]
    public void Validator_Should_Have_Errors_For_Invalid_Command()
    {
        var validator = new CreateOrderCommandValidator();

        var command = new CreateOrderCommand(
            Items: [],
            CustomerName: "",
            CustomerPhone: null,
            DeliveryAddress: "",
            Payments: [],
            DeliveryFee: -1m,
            DeliveryReference: null,
            Notes: null
        );

        var result = validator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.Any(e => e.PropertyName == "Items").ShouldBeTrue();
        result.Errors.Any(e => e.PropertyName == "Payments").ShouldBeTrue();
        result.Errors.Any(e => e.PropertyName == "CustomerName").ShouldBeTrue();
        result.Errors.Any(e => e.PropertyName == "DeliveryAddress").ShouldBeTrue();
        result.Errors.Any(e => e.PropertyName == "DeliveryFee").ShouldBeTrue();
    }

    [Fact]
    public void Validator_Should_Use_Portuguese_Field_Names_And_Keep_Property_Paths()
    {
        // Arrange
        var validator = new CreateOrderCommandValidator();
        var command = new CreateOrderCommand(
            Items: [new OrderItemDto(Guid.NewGuid(), 0, null)],
            CustomerName: "Cliente Teste",
            CustomerPhone: new string('9', 21),
            DeliveryAddress: "Rua Teste, 123",
            Payments: [new OrderPaymentDto((PaymentMethod)99, 10m)],
            Notes: new string('a', 501));

        // Act
        var result = validator.Validate(command);

        // Assert
        result.Errors.ToDictionary(e => e.PropertyName, e => e.ErrorMessage).ShouldBe(
            new Dictionary<string, string>
            {
                ["Items[0].Quantity"] = "O campo 'Quantidade' deve ser maior que 0.",
                ["CustomerPhone"] = "O campo 'Telefone do cliente' deve ter no máximo 20 caracteres.",
                ["Payments[0].Method"] = "O campo 'Forma de pagamento' possui um valor inválido.",
                ["Notes"] = "O campo 'Observações' deve ter no máximo 500 caracteres."
            },
            ignoreOrder: true);
    }

    [Fact]
    public void Validator_Should_Pass_For_Valid_Command()
    {
        var validator = new CreateOrderCommandValidator();
        var items = new[] { new OrderItemDto(Guid.NewGuid(), 1, null) };
        var payments = new[] { new OrderPaymentDto(PaymentMethod.Cash, 10m) };

        var command = new CreateOrderCommand(
            Items: items,
            CustomerName: "Cliente Teste",
            CustomerPhone: "11999999999",
            DeliveryAddress: "Rua Teste, 123",
            Payments: payments,
            DeliveryFee: 5m,
            DeliveryReference: null,
            Notes: ""
        );

        var result = validator.Validate(command);

        result.IsValid.ShouldBeTrue();
    }
}