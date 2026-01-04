using Devlivery.Features.CashRegister.Commands.CloseCashSession;

using FluentValidation.TestHelper;

namespace Devlivery.Tests.Features.CashRegister.Commands.CloseCashSession;

[Trait("Category", "Unit Tests")]
public sealed class CloseCashSessionValidatorTests
{
    private readonly CloseCashSessionValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        // Arrange
        var command = new CloseCashSessionCommand(
            Guid.Empty,
            100m,
            null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("O id do caixa é obrigatório.");
    }

    [Fact]
    public void Should_Have_Error_When_ClosingAmount_Is_Negative()
    {
        // Arrange
        var command = new CloseCashSessionCommand(
            Guid.NewGuid(),
            -10m,
            null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClosingAmount)
            .WithErrorMessage("O valor de fechamento deve ser maior ou igual a zero.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new CloseCashSessionCommand(
            Guid.NewGuid(),
            250m,
            "Fechamento normal");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_ClosingAmount_Is_Zero()
    {
        // Arrange
        var command = new CloseCashSessionCommand(
            Guid.NewGuid(),
            0m,
            null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ClosingAmount);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Notes_Is_Null()
    {
        // Arrange
        var command = new CloseCashSessionCommand(
            Guid.NewGuid(),
            100m,
            null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}