using Devlivery.Features.Expenses.Commands.CreateCategory;

using FluentValidation.TestHelper;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.CreateCategory;

[Trait("Category", "Unit Tests")]
[Collection("Expenses Unit Tests")]
public sealed class CreateCategoryCommandValidatorTests
{
    [Fact]
    public void Validator_Should_Have_Error_When_Name_Is_Empty()
    {
        var validator = new CreateCategoryCommandValidator();
        var command = new CreateCategoryCommand("");

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Validator_Should_Have_Error_When_Name_Too_Long()
    {
        var validator = new CreateCategoryCommandValidator();
        var longName = new string('a', 201);
        var command = new CreateCategoryCommand(longName);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Validator_Should_Pass_For_Valid_Name()
    {
        var validator = new CreateCategoryCommandValidator();
        var command = new CreateCategoryCommand("Nome Válido");

        var result = validator.TestValidate(command);

        result.IsValid.ShouldBeTrue();
    }
}