using Devlivery.Features.Expenses.Commands.UpdateCategory;

using FluentValidation.TestHelper;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.UpdateCategory;

[Trait("Category", "Unit Tests")]
[Collection("Expenses Unit Tests")]
public sealed class UpdateCategoryCommandValidatorTests
{
    [Fact]
    public void Validator_Should_Have_Error_When_CategoryId_Is_Empty()
    {
        var validator = new UpdateCategoryCommandValidator();
        var command = new UpdateCategoryCommand(Guid.Empty, null, null);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.CategoryId);
    }

    [Fact]
    public void Validator_Should_Have_Error_When_Name_Provided_But_Empty()
    {
        var validator = new UpdateCategoryCommandValidator();
        var command = new UpdateCategoryCommand(Guid.NewGuid(), "", null);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Validator_Should_Have_Error_When_Name_Too_Long()
    {
        var validator = new UpdateCategoryCommandValidator();
        var longName = new string('a', 201);
        var command = new UpdateCategoryCommand(Guid.NewGuid(), longName, null);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Validator_Should_Pass_When_Name_Not_Provided()
    {
        var validator = new UpdateCategoryCommandValidator();
        var command = new UpdateCategoryCommand(Guid.NewGuid(), null, null);

        var result = validator.TestValidate(command);

        result.IsValid.ShouldBeTrue();
    }
}