using Devlivery.Features.Expenses.Commands.CreateCategory;

using FluentValidation.TestHelper;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.CreateCategory;

[Trait("Category", "Unit Tests")]
public sealed class CreateCategoryValidatorTests
{
    private static readonly CreateCategoryCommand ValidCommand = new("Operacional");

    private readonly CreateCategoryCommandValidator _validator = new();

    public static TheoryData<CreateCategoryCommand, string, string> InvalidCommands => new()
    {
        { ValidCommand with { Name = "" }, "Name", "O campo 'Nome' é obrigatório." },
        { ValidCommand with { Name = new string('A', 201) }, "Name", "O campo 'Nome' deve ter no máximo 200 caracteres." }
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
        CreateCategoryCommand command, string propertyName, string expectedMessage)
    {
        // Arrange: command comes from InvalidCommands

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(propertyName).WithErrorMessage(expectedMessage);
    }
}