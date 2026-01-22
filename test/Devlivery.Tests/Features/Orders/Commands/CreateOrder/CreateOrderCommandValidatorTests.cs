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
