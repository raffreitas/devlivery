using Devlivery.Domain.Common.Enums;
using Devlivery.Features.Orders.Commands.CreateOrder;

using FluentValidation.TestHelper;

using Shouldly;

namespace Devlivery.Tests.Features.Orders.Commands.CreateOrder;

[Trait("Category", "Unit Tests")]
public sealed class CreateOrderValidatorTests
{
    private static readonly CreateOrderCommand ValidCommand = new(
        Items: [new OrderItemDto(Guid.NewGuid(), 1, null)],
        CustomerName: "João Silva",
        CustomerPhone: "11987654321",
        DeliveryAddress: "Rua das Flores, 123",
        Payments: [new OrderPaymentDto(PaymentMethod.Cash, 10m)],
        DeliveryFee: 5m);

    private readonly CreateOrderCommandValidator _validator = new();

    public static TheoryData<CreateOrderCommand, string, string> InvalidCommands => new()
    {
        { ValidCommand with { Items = [] }, "Items", "O campo 'Itens' é obrigatório." },
        { ValidCommand with { Items = [new OrderItemDto(Guid.Empty, 1, null)] }, "Items[0].ProductId", "O campo 'Produto' é obrigatório." },
        { ValidCommand with { Items = [new OrderItemDto(Guid.NewGuid(), 0, null)] }, "Items[0].Quantity", "O campo 'Quantidade' deve ser maior que 0." },
        { ValidCommand with { CustomerName = "" }, "CustomerName", "O campo 'Nome do cliente' é obrigatório." },
        { ValidCommand with { CustomerName = new string('A', 201) }, "CustomerName", "O campo 'Nome do cliente' deve ter no máximo 200 caracteres." },
        { ValidCommand with { CustomerPhone = new string('9', 21) }, "CustomerPhone", "O campo 'Telefone do cliente' deve ter no máximo 20 caracteres." },
        { ValidCommand with { Payments = [] }, "Payments", "O campo 'Pagamentos' é obrigatório." },
        { ValidCommand with { Payments = [new OrderPaymentDto((PaymentMethod)99, 10m)] }, "Payments[0].Method", "O campo 'Forma de pagamento' possui um valor inválido." },
        { ValidCommand with { Payments = [new OrderPaymentDto(PaymentMethod.Pix, 0m)] }, "Payments[0].Amount", "O campo 'Valor do pagamento' deve ser maior que 0." },
        { ValidCommand with { DeliveryAddress = "" }, "DeliveryAddress", "O campo 'Endereço de entrega' é obrigatório." },
        { ValidCommand with { DeliveryAddress = new string('A', 501) }, "DeliveryAddress", "O campo 'Endereço de entrega' deve ter no máximo 500 caracteres." },
        { ValidCommand with { DeliveryFee = -1m }, "DeliveryFee", "O campo 'Taxa de entrega' deve ser maior ou igual a 0." },
        { ValidCommand with { Notes = new string('A', 501) }, "Notes", "O campo 'Observações' deve ter no máximo 500 caracteres." }
    };

    [Fact]
    public void GivenValidCommand_WhenValidating_ThenHasNoErrors()
    {
        // Arrange
        var command = ValidCommand;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [MemberData(nameof(InvalidCommands))]
    public void GivenInvalidField_WhenValidating_ThenReturnsPortugueseErrorForField(
        CreateOrderCommand command, string propertyName, string expectedMessage)
    {
        // Arrange: command comes from InvalidCommands

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(propertyName).WithErrorMessage(expectedMessage);
    }
}