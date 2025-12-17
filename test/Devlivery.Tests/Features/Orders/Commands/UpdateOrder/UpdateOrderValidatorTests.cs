using Devlivery.Features.Orders.Commands.UpdateOrder;
using Devlivery.Features.Orders.Domain.Enums;

using FluentValidation.TestHelper;

using Shouldly;

namespace Devlivery.Tests.Features.Orders.Commands.UpdateOrder;

[Collection("Orders Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class UpdateOrderValidatorTests(OrdersUnitTestFixture fixture)
{
    private readonly UpdateOrderCommandValidator _validator = new();

    [Fact]
    public void Should_Pass_Validation_When_Command_Is_Valid()
    {
        // Arrange
        var command = new UpdateOrderCommand(
            Id: Guid.NewGuid(),
            Items: [new OrderItemDto(Guid.NewGuid(), 2, null)],
            CustomerName: fixture.Faker.Person.FullName,
            CustomerPhone: fixture.Faker.Phone.PhoneNumber(),
            DeliveryAddress: fixture.Faker.Address.FullAddress(),
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
    public void Should_Fail_Validation_When_Id_Is_Empty()
    {
        // Arrange
        var command = new UpdateOrderCommand(
            Id: Guid.Empty,
            Items: [new OrderItemDto(Guid.NewGuid(), 2, null)],
            CustomerName: "Cliente Teste",
            CustomerPhone: "123456789",
            DeliveryAddress: "Endereço Teste",
            PaymentMethod: PaymentMethod.Cash
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("O campo 'Id' é obrigatório.");
    }

    [Fact]
    public void Should_Fail_Validation_When_Items_Array_Is_Empty()
    {
        // Arrange
        var command = new UpdateOrderCommand(
            Id: Guid.NewGuid(),
            Items: [],
            CustomerName: "Cliente Teste",
            CustomerPhone: "123456789",
            DeliveryAddress: "Endereço Teste",
            PaymentMethod: PaymentMethod.Cash
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Items)
            .WithErrorMessage("O campo 'Items' não pode estar vazio.");
    }

    [Fact]
    public void Should_Fail_Validation_When_CustomerName_Is_Empty()
    {
        // Arrange
        var command = new UpdateOrderCommand(
            Id: Guid.NewGuid(),
            Items: [new OrderItemDto(Guid.NewGuid(), 2, null)],
            CustomerName: string.Empty,
            CustomerPhone: "123456789",
            DeliveryAddress: "Endereço Teste",
            PaymentMethod: PaymentMethod.Cash
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
        var command = new UpdateOrderCommand(
            Id: Guid.NewGuid(),
            Items: [new OrderItemDto(Guid.NewGuid(), 2, null)],
            CustomerName: new string('A', 201),
            CustomerPhone: "123456789",
            DeliveryAddress: "Endereço Teste",
            PaymentMethod: PaymentMethod.Cash
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CustomerName)
            .WithErrorMessage("O campo 'Customer Name' deve ter no máximo 200 caracteres.");
    }

    [Fact]
    public void Should_Fail_Validation_When_DeliveryAddress_Is_Empty()
    {
        // Arrange
        var command = new UpdateOrderCommand(
            Id: Guid.NewGuid(),
            Items: [new OrderItemDto(Guid.NewGuid(), 2, null)],
            CustomerName: "Cliente Teste",
            CustomerPhone: "123456789",
            DeliveryAddress: string.Empty,
            PaymentMethod: PaymentMethod.Cash
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor(x => x.DeliveryAddress)
            .WithErrorMessage("O campo 'Delivery Address' é obrigatório.");
    }

    [Fact]
    public void Should_Fail_Validation_When_DeliveryFee_Is_Negative()
    {
        // Arrange
        var command = new UpdateOrderCommand(
            Id: Guid.NewGuid(),
            Items: [new OrderItemDto(Guid.NewGuid(), 2, null)],
            CustomerName: "Cliente Teste",
            CustomerPhone: "123456789",
            DeliveryAddress: "Endereço Teste",
            PaymentMethod: PaymentMethod.Cash,
            DeliveryFee: -5.00m
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor(x => x.DeliveryFee)
            .WithErrorMessage("O campo 'Delivery Fee' deve ser maior ou igual a 0.");
    }
}