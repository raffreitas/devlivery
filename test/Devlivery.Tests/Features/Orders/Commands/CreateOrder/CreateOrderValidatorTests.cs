using Devlivery.Features.Orders.Commands.CreateOrder;
using Devlivery.Features.Orders.Domain.Enums;

using FluentValidation.TestHelper;

using Shouldly;

namespace Devlivery.Tests.Features.Orders.Commands.CreateOrder;

[Collection("Orders Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class CreateOrderValidatorTests(OrdersUnitTestFixture fixture)
{
    private readonly CreateOrderCommandValidator _validator = new();

    [Fact]
    public void Should_Pass_Validation_When_Command_Is_Valid()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Items: [new OrderItemDto(Guid.NewGuid(), 2, null)],
            CustomerName: fixture.Faker.Person.FullName,
            CustomerPhone: fixture.Faker.Phone.PhoneNumber(),
            DeliveryAddress: fixture.Faker.Address.FullAddress(),
            DeliveryReference: null,
            PaymentMethod: PaymentMethod.Pix,
            DeliveryFee: 5.00m,
            Notes: fixture.Faker.Lorem.Sentence()
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Fail_Validation_When_Items_Array_Is_Empty()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Items: [],
            CustomerName: "Cliente Teste",
            CustomerPhone: "123456789",
            DeliveryAddress: "Endereço Teste",
            DeliveryReference: null,
            PaymentMethod:
            PaymentMethod.Cash
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Items)
            .WithErrorMessage("O campo 'Items' não pode estar vazio.");
    }

    [Fact]
    public void Should_Fail_Validation_When_Item_ProductId_Is_Empty()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Items: [new OrderItemDto(Guid.Empty, 2, null)],
            CustomerName: "Cliente Teste",
            CustomerPhone: "123456789",
            DeliveryAddress: "Endereço Teste",
            DeliveryReference: null,
            PaymentMethod:
            PaymentMethod.Cash
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor("Items[0].ProductId")
            .WithErrorMessage("O campo 'Product Id' é obrigatório.");
    }

    [Fact]
    public void Should_Fail_Validation_When_Item_Quantity_Is_Zero()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Items: [new OrderItemDto(Guid.NewGuid(), 0, null)],
            CustomerName: "Cliente Teste",
            CustomerPhone: "123456789",
            DeliveryAddress: "Endereço Teste",
            DeliveryReference: null,
            PaymentMethod:
            PaymentMethod.Cash
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor("Items[0].Quantity")
            .WithErrorMessage("O campo 'Quantity' deve ser maior que 0.");
    }

    [Fact]
    public void Should_Fail_Validation_When_Item_Quantity_Is_Negative()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Items: [new OrderItemDto(Guid.NewGuid(), -1, null)],
            CustomerName: "Cliente Teste",
            CustomerPhone: "123456789",
            DeliveryAddress: "Endereço Teste",
            DeliveryReference: null,
            PaymentMethod:
            PaymentMethod.Cash
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor("Items[0].Quantity")
            .WithErrorMessage("O campo 'Quantity' deve ser maior que 0.");
    }

    [Fact]
    public void Should_Fail_Validation_When_CustomerName_Is_Empty()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Items: [new OrderItemDto(Guid.NewGuid(), 2, null)],
            CustomerName: string.Empty,
            CustomerPhone: "123456789",
            DeliveryAddress: "Endereço Teste",
            DeliveryReference: null,
            PaymentMethod:
            PaymentMethod.Cash
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CustomerName)
            .WithErrorMessage("O campo 'Customer Name' é obrigatório.");
    }

    [Fact]
    public void Should_Fail_Validation_When_CustomerName_Exceeds_MaxLength()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Items: [new OrderItemDto(Guid.NewGuid(), 2, null)],
            CustomerName: new string('A', 201), // 201 caracteres
            CustomerPhone: "123456789",
            DeliveryAddress: "Endereço Teste",
            DeliveryReference: null,
            PaymentMethod:
            PaymentMethod.Cash
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CustomerName)
            .WithErrorMessage("O campo 'Customer Name' deve ter no máximo 200 caracteres.");
    }

    [Fact]
    public void Should_Fail_Validation_When_CustomerPhone_Exceeds_MaxLength()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Items: [new OrderItemDto(Guid.NewGuid(), 2, null)],
            CustomerName: "Cliente Teste",
            CustomerPhone: new string('1', 21), // 21 caracteres
            DeliveryAddress: "Endereço Teste",
            DeliveryReference: null,
            PaymentMethod:
            PaymentMethod.Cash
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CustomerPhone)
            .WithErrorMessage("O campo 'Customer Phone' deve ter no máximo 20 caracteres.");
    }

    [Fact]
    public void Should_Pass_Validation_When_CustomerPhone_Is_Null()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Items: [new OrderItemDto(Guid.NewGuid(), 2, null)],
            CustomerName: "Cliente Teste",
            CustomerPhone: null,
            DeliveryAddress: "Endereço Teste",
            DeliveryReference: null,
            PaymentMethod:
            PaymentMethod.Cash
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Should_Fail_Validation_When_DeliveryAddress_Is_Empty()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Items: [new OrderItemDto(Guid.NewGuid(), 2, null)],
            CustomerName: "Cliente Teste",
            CustomerPhone: "123456789",
            DeliveryAddress: string.Empty,
            DeliveryReference: null,
            PaymentMethod:
            PaymentMethod.Cash
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor(x => x.DeliveryAddress)
            .WithErrorMessage("O campo 'Delivery Address' é obrigatório.");
    }

    [Fact]
    public void Should_Fail_Validation_When_DeliveryAddress_Exceeds_MaxLength()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Items: [new OrderItemDto(Guid.NewGuid(), 2, null)],
            CustomerName: "Cliente Teste",
            CustomerPhone: "123456789",
            DeliveryAddress: new string('A', 501), // 501 caracteres
            DeliveryReference: null,
            PaymentMethod: PaymentMethod.Cash
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor(x => x.DeliveryAddress)
            .WithErrorMessage("O campo 'Delivery Address' deve ter no máximo 500 caracteres.");
    }

    [Fact]
    public void Should_Fail_Validation_When_DeliveryFee_Is_Negative()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Items: [new OrderItemDto(Guid.NewGuid(), 2, null)],
            CustomerName: "Cliente Teste",
            CustomerPhone: "123456789",
            DeliveryAddress: "Endereço Teste",
            DeliveryReference: null,
            PaymentMethod:
            PaymentMethod.Cash,
            DeliveryFee:
            -5.00m
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor(x => x.DeliveryFee)
            .WithErrorMessage("O campo 'Delivery Fee' deve ser maior ou igual a 0.");
    }

    [Fact]
    public void Should_Pass_Validation_When_DeliveryFee_Is_Zero()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Items: [new OrderItemDto(Guid.NewGuid(), 2, null)],
            CustomerName: "Cliente Teste",
            CustomerPhone: "123456789",
            DeliveryAddress: "Endereço Teste",
            DeliveryReference: null,
            PaymentMethod:
            PaymentMethod.Cash,
            DeliveryFee:
            0m
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Should_Fail_Validation_When_Notes_Exceeds_MaxLength()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Items: [new OrderItemDto(Guid.NewGuid(), 2, null)],
            CustomerName: "Cliente Teste",
            CustomerPhone: "123456789",
            DeliveryAddress: "Endereço Teste",
            DeliveryReference: null,
            PaymentMethod:
            PaymentMethod.Cash,
            Notes:
            new string('A', 501) // 501 caracteres
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Notes)
            .WithErrorMessage("O campo 'Notes' deve ter no máximo 500 caracteres.");
    }

    [Fact]
    public void Should_Pass_Validation_When_Notes_Is_Null()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Items: [new OrderItemDto(Guid.NewGuid(), 2, null)],
            CustomerName: "Cliente Teste",
            CustomerPhone: "123456789",
            DeliveryAddress: "Endereço Teste",
            DeliveryReference: null,
            PaymentMethod:
            PaymentMethod.Cash,
            Notes:
            null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(PaymentMethod.Cash)]
    [InlineData(PaymentMethod.CreditCard)]
    [InlineData(PaymentMethod.DebitCard)]
    [InlineData(PaymentMethod.Pix)]
    public void Should_Pass_Validation_For_All_Valid_PaymentMethods(PaymentMethod paymentMethod)
    {
        // Arrange
        var command = new CreateOrderCommand(
            Items: [new OrderItemDto(Guid.NewGuid(), 2, null)],
            CustomerName: "Cliente Teste",
            CustomerPhone: "123456789",
            DeliveryAddress: "Endereço Teste",
            DeliveryReference: null,
            PaymentMethod:
            paymentMethod
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }
}