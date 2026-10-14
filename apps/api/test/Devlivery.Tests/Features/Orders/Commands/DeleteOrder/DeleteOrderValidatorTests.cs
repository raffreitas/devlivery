using Devlivery.Features.Orders.Commands.DeleteOrder;

using FluentValidation.TestHelper;

namespace Devlivery.Tests.Features.Orders.Commands.DeleteOrder;

[Trait("Category", "Unit Tests")]
public sealed class DeleteOrderValidatorTests
{
    private readonly DeleteOrderCommandValidator _validator = new();

    [Fact]
    public void GivenEmptyId_WhenValidating_ThenReturnsRequiredOrderError()
    {
        // Arrange
        var command = new DeleteOrderCommand(Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id).WithErrorMessage("O campo 'Pedido' é obrigatório.");
    }
}