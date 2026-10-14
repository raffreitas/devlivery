using Devlivery.Features.CashRegister.Commands.CreateCashDeposit;

using FluentValidation.TestHelper;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Commands.CreateCashDeposit;

[Trait("Category", "Unit Tests")]
public sealed class CreateCashDepositValidatorTests
{
    private static readonly CreateCashDepositCommand ValidCommand = new(Guid.NewGuid(), 0.01m, null);

    private readonly CreateCashDepositValidator _validator = new();

    public static TheoryData<CreateCashDepositCommand, string, string> InvalidCommands => new()
    {
        { ValidCommand with { CashSessionId = Guid.Empty }, "CashSessionId", "O campo 'Caixa' é obrigatório." },
        { ValidCommand with { Amount = 0m }, "Amount", "O campo 'Valor do aporte' deve ser maior que 0." },
        { ValidCommand with { Amount = -10m }, "Amount", "O campo 'Valor do aporte' deve ser maior que 0." }
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
        CreateCashDepositCommand command, string propertyName, string expectedMessage)
    {
        // Arrange: command comes from InvalidCommands

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(propertyName).WithErrorMessage(expectedMessage);
    }
}