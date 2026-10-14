using Devlivery.Features.Expenses.Commands.DeleteCategory;

using FluentValidation.TestHelper;

namespace Devlivery.Tests.Features.Expenses.Commands.DeleteCategory;

[Trait("Category", "Unit Tests")]
public sealed class DeleteCategoryValidatorTests
{
    private readonly DeleteCategoryCommandValidator _validator = new();

    [Fact]
    public void GivenEmptyCategoryId_WhenValidating_ThenReturnsRequiredCategoryError()
    {
        // Arrange
        var command = new DeleteCategoryCommand(Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryId)
            .WithErrorMessage("O campo 'Categoria' é obrigatório.");
    }
}