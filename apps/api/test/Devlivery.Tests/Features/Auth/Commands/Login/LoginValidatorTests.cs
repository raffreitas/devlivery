using Devlivery.Features.Auth.Commands.Login;

using FluentValidation.TestHelper;

namespace Devlivery.Tests.Features.Auth.Commands.Login;

[Collection("Auth Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class LoginValidatorTests(AuthUnitTestFixture fixture) : IClassFixture<AuthUnitTestFixture>
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new LoginCommand(
            Email: fixture.Faker.Internet.Email(),
            Password: fixture.Faker.Internet.Password(8)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        // Arrange
        var command = new LoginCommand(
            Email: string.Empty,
            Password: fixture.Faker.Internet.Password()
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("O campo 'Email' é obrigatório.");
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        // Arrange
        var command = new LoginCommand(
            Email: "invalid-email",
            Password: fixture.Faker.Internet.Password()
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("O campo 'Email' deve ser um e-mail válido.");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Empty()
    {
        // Arrange
        var command = new LoginCommand(
            Email: fixture.Faker.Internet.Email(),
            Password: string.Empty
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("O campo 'Password' é obrigatório.");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Too_Short()
    {
        // Arrange
        var command = new LoginCommand(
            Email: fixture.Faker.Internet.Email(),
            Password: "12345" // Less than 6 characters
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("O campo 'Password' deve ter no mínimo 6 caracteres.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Should_Have_Error_When_Email_Is_Null_Or_Whitespace(string? email)
    {
        // Arrange
        var command = new LoginCommand(
            Email: email!,
            Password: fixture.Faker.Internet.Password()
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Should_Have_Error_When_Password_Is_Null_Or_Whitespace(string? password)
    {
        // Arrange
        var command = new LoginCommand(
            Email: fixture.Faker.Internet.Email(),
            Password: password!
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
