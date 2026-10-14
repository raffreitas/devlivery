using Devlivery.Features.CashRegister.Commands.CreateCashSession;

using FluentValidation.TestHelper;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Commands.CreateCashSession;

[Trait("Category", "Unit Tests")]
public sealed class CreateCashSessionValidatorTests
{
    private readonly CreateCashSessionValidator _validator = new();

    [Fact]
    public void GivenZeroOpeningAmount_WhenValidating_ThenHasNoErrors()
    {
        // Arrange
        var command = new CreateCashSessionCommand(0m, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void GivenNegativeOpeningAmount_WhenValidating_ThenReturnsPortugueseError()
    {
        // Arrange
        var command = new CreateCashSessionCommand(-0.01m, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OpeningAmount)
            .WithErrorMessage("O campo 'Valor de abertura' deve ser maior ou igual a 0.");
    }
}