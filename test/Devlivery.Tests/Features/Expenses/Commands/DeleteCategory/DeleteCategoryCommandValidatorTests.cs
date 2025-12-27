using Devlivery.Features.Expenses.Commands.DeleteCategory;

using FluentValidation.TestHelper;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.DeleteCategory;

[Trait("Category", "Unit Tests")]
[Collection("Expenses Unit Tests")]
public sealed class DeleteCategoryCommandValidatorTests()
{
    [Fact]
    public void Validator_Should_Have_Error_When_CategoryId_Is_Empty()
    {
        var validator = new DeleteCategoryCommandValidator();
        var command = new DeleteCategoryCommand(Guid.Empty);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.CategoryId);
    }

    [Fact]
    public void Validator_Should_Pass_For_Valid_CategoryId()
    {
        var validator = new DeleteCategoryCommandValidator();
        var command = new DeleteCategoryCommand(Guid.NewGuid());

        var result = validator.TestValidate(command);

        result.IsValid.ShouldBeTrue();
    }
}