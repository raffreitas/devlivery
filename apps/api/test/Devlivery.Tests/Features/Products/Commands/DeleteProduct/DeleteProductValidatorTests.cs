using Devlivery.Features.Products.Commands.DeleteProduct;

using FluentValidation.TestHelper;

namespace Devlivery.Tests.Features.Products.Commands.DeleteProduct;

[Trait("Category", "Unit Tests")]
public sealed class DeleteProductValidatorTests
{
    private readonly DeleteProductCommandValidator _validator = new();

    [Fact]
    public void GivenEmptyId_WhenValidating_ThenReturnsRequiredProductError()
    {
        // Arrange
        var command = new DeleteProductCommand(Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id).WithErrorMessage("O campo 'Produto' é obrigatório.");
    }
}