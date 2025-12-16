using Devlivery.Features.Products.Commands.DeleteProduct;

using FluentValidation.TestHelper;

using Shouldly;

namespace Devlivery.Tests.Features.Products.Commands.DeleteProduct;

[Collection("Products Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class DeleteProductValidatorTests
{
    private readonly DeleteProductCommandValidator _validator = new();

    [Fact]
    public void Should_Pass_Validation_When_Command_Is_Valid()
    {
        // Arrange
        var command = new DeleteProductCommand(Id: Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Fail_Validation_When_Id_Is_Empty()
    {
        // Arrange
        var command = new DeleteProductCommand(Id: Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("O campo 'Id' é obrigatório.");
    }
}