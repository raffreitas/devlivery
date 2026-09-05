using Devlivery.Features.CashRegister.Commands.CreateCashSession;

using FluentValidation.TestHelper;

namespace Devlivery.Tests.Features.CashRegister.Commands.CreateCashSession;

public sealed class CreateCashSessionValidatorTests
{
    private readonly CreateCashSessionValidator _validator = new();




    [Fact]
    public void Should_Have_Error_When_OpeningAmount_Is_Negative()
    {
        // Arrange
        var command = new CreateCashSessionCommand(-10m, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OpeningAmount)
            .WithErrorMessage("O valor de abertura deve ser maior ou igual a zero.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new CreateCashSessionCommand(100m, "Abertura de caixa");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_OpeningAmount_Is_Zero()
    {
        // Arrange
        var command = new CreateCashSessionCommand(0m, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.OpeningAmount);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Notes_Is_Null()
    {
        // Arrange
        var command = new CreateCashSessionCommand(100m, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}