using Devlivery.Features.Expenses.Commands.UpdateCategory;

using FluentValidation.TestHelper;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.UpdateCategory;

[Trait("Category", "Unit Tests")]
public sealed class UpdateCategoryValidatorTests
{
    private static readonly UpdateCategoryCommand ValidCommand = new(Guid.NewGuid(), Name: null, IsActive: false);

    private readonly UpdateCategoryCommandValidator _validator = new();

    public static TheoryData<UpdateCategoryCommand, string, string> InvalidCommands => new()
    {
        { ValidCommand with { CategoryId = Guid.Empty }, "CategoryId", "O campo 'Categoria' é obrigatório." },
        { ValidCommand with { Name = "" }, "Name", "O campo 'Nome' é obrigatório." },
        { ValidCommand with { Name = new string('A', 201) }, "Name", "O campo 'Nome' deve ter no máximo 200 caracteres." }
    };

    [Fact]
    public void GivenCommandWithoutName_WhenValidating_ThenHasNoErrors()
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
        UpdateCategoryCommand command, string propertyName, string expectedMessage)
    {
        // Arrange: command comes from InvalidCommands

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(propertyName).WithErrorMessage(expectedMessage);
    }
}