using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Features.Orders.Commands.UpdateOrderStatus;

using FluentValidation.TestHelper;

using Shouldly;

namespace Devlivery.Tests.Features.Orders.Commands.UpdateOrderStatus;

[Trait("Category", "Unit Tests")]
public sealed class UpdateOrderStatusValidatorTests
{
    private static readonly UpdateOrderStatusCommand ValidCommand = new(Guid.NewGuid(), OrderStatus.Preparing);

    private readonly UpdateOrderStatusCommandValidator _validator = new();

    public static TheoryData<UpdateOrderStatusCommand, string, string> InvalidCommands => new()
    {
        { ValidCommand with { Id = Guid.Empty }, "Id", "O campo 'Pedido' é obrigatório." },
        { ValidCommand with { Status = (OrderStatus)99 }, "Status", "O campo 'Status' possui um valor inválido." }
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
        UpdateOrderStatusCommand command, string propertyName, string expectedMessage)
    {
        // Arrange: command comes from InvalidCommands

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(propertyName).WithErrorMessage(expectedMessage);
    }
}