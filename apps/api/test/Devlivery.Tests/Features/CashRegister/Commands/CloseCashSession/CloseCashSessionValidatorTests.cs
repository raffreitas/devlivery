using Devlivery.Features.CashRegister.Commands.CloseCashSession;

using FluentValidation.TestHelper;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Commands.CloseCashSession;

[Trait("Category", "Unit Tests")]
public sealed class CloseCashSessionValidatorTests
{
    private static readonly CloseCashSessionCommand ValidCommand = new(Guid.NewGuid(), 0m, null);

    private readonly CloseCashSessionValidator _validator = new();

    public static TheoryData<CloseCashSessionCommand, string, string> InvalidCommands => new()
    {
        { ValidCommand with { Id = Guid.Empty }, "Id", "O campo 'Caixa' é obrigatório." },
        { ValidCommand with { ClosingAmount = -0.01m }, "ClosingAmount", "O campo 'Valor de fechamento' deve ser maior ou igual a 0." }
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
        CloseCashSessionCommand command, string propertyName, string expectedMessage)
    {
        // Arrange: command comes from InvalidCommands

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(propertyName).WithErrorMessage(expectedMessage);
    }
}