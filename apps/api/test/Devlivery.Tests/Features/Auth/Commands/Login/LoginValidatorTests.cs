using Devlivery.Features.Auth.Commands.Login;

using FluentValidation.TestHelper;

using Shouldly;

namespace Devlivery.Tests.Features.Auth.Commands.Login;

[Trait("Category", "Unit Tests")]
public sealed class LoginValidatorTests
{
    private static readonly LoginCommand ValidCommand = new("maria@pizzaria.com", "123456");

    private readonly LoginCommandValidator _validator = new();

    public static TheoryData<LoginCommand, string, string> InvalidCommands => new()
    {
        { ValidCommand with { Email = "" }, "Email", "O campo 'E-mail' é obrigatório." },
        { ValidCommand with { Email = "   " }, "Email", "O campo 'E-mail' é obrigatório." },
        { ValidCommand with { Email = null! }, "Email", "O campo 'E-mail' é obrigatório." },
        { ValidCommand with { Email = "maria.pizzaria.com" }, "Email", "O campo 'E-mail' deve ser um e-mail válido." },
        { ValidCommand with { Password = "" }, "Password", "O campo 'Senha' é obrigatório." },
        { ValidCommand with { Password = "   " }, "Password", "O campo 'Senha' é obrigatório." },
        { ValidCommand with { Password = null! }, "Password", "O campo 'Senha' é obrigatório." },
        { ValidCommand with { Password = "12345" }, "Password", "O campo 'Senha' deve ter no mínimo 6 caracteres." }
    };

    [Fact]
    public void GivenEmailAndSixCharacterPassword_WhenValidating_ThenHasNoErrors()
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
        LoginCommand command, string propertyName, string expectedMessage)
    {
        // Arrange: command comes from InvalidCommands

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(propertyName).WithErrorMessage(expectedMessage);
    }
}