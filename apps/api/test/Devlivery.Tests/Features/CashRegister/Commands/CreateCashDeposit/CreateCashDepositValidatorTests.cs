using Devlivery.Features.CashRegister.Commands.CreateCashDeposit;

using FluentValidation.TestHelper;

namespace Devlivery.Tests.Features.CashRegister.Commands.CreateCashDeposit;

public sealed class CreateCashDepositValidatorTests
{
    private readonly CreateCashDepositValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_CashSessionId_Is_Empty()
    {
        // Arrange
        var command = new CreateCashDepositCommand(Guid.Empty, 50m, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CashSessionId)
            .WithErrorMessage("A sessão de caixa é obrigatória.");
    }




    [Fact]
    public void Should_Have_Error_When_Amount_Is_Zero()
    {
        // Arrange
        var command = new CreateCashDepositCommand(Guid.NewGuid(), 0m, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorMessage("O valor do aporte deve ser maior que zero.");
    }

    [Fact]
    public void Should_Have_Error_When_Amount_Is_Negative()
    {
        // Arrange
        var command = new CreateCashDepositCommand(Guid.NewGuid(), -10m, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorMessage("O valor do aporte deve ser maior que zero.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new CreateCashDepositCommand(Guid.NewGuid(), 50m, "Aporte inicial");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Notes_Is_Null()
    {
        // Arrange
        var command = new CreateCashDepositCommand(Guid.NewGuid(), 50m, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}