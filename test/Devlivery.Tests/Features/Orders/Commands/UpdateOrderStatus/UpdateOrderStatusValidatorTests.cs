using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Features.Orders.Commands.UpdateOrderStatus;

using FluentValidation.TestHelper;

using Shouldly;

namespace Devlivery.Tests.Features.Orders.Commands.UpdateOrderStatus;

[Collection("Orders Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class UpdateOrderStatusValidatorTests
{
    private readonly UpdateOrderStatusCommandValidator _validator = new();

    [Fact]
    public void Should_Pass_Validation_When_Command_Is_Valid()
    {
        // Arrange
        var command = new UpdateOrderStatusCommand(Guid.NewGuid(), OrderStatus.Preparing);

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
        var command = new UpdateOrderStatusCommand(Guid.Empty, OrderStatus.Preparing);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("O campo 'Id' é obrigatório.");
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Preparing)]
    [InlineData(OrderStatus.Ready)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Canceled)]
    public void Should_Pass_Validation_For_All_Valid_OrderStatuses(OrderStatus status)
    {
        // Arrange
        var command = new UpdateOrderStatusCommand(Guid.NewGuid(), status);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }
}