using Devlivery.Features.Products.Commands.CreateProduct;

using FluentValidation.TestHelper;

using Shouldly;

namespace Devlivery.Tests.Features.Products.Commands.CreateProduct;

[Trait("Category", "Unit Tests")]
public sealed class CreateProductValidatorTests
{
    private static readonly CreateProductCommand ValidCommand =
        new("Pizza Calabresa", "Molho, calabresa e cebola", 49.90m, "Pizzas", Available: true);

    private readonly CreateProductCommandValidator _validator = new();

    public static TheoryData<CreateProductCommand, string, string> InvalidCommands => new()
    {
        { ValidCommand with { Name = "" }, "Name", "O campo 'Nome' é obrigatório." },
        { ValidCommand with { Name = new string('A', 201) }, "Name", "O campo 'Nome' deve ter no máximo 200 caracteres." },
        { ValidCommand with { Description = "" }, "Description", "O campo 'Descrição' é obrigatório." },
        { ValidCommand with { Description = new string('A', 1001) }, "Description", "O campo 'Descrição' deve ter no máximo 1000 caracteres." },
        { ValidCommand with { Price = 0m }, "Price", "O campo 'Preço' deve ser maior que 0." },
        { ValidCommand with { Price = -10m }, "Price", "O campo 'Preço' deve ser maior que 0." },
        { ValidCommand with { Category = "" }, "Category", "O campo 'Categoria' é obrigatório." },
        { ValidCommand with { Category = new string('A', 101) }, "Category", "O campo 'Categoria' deve ter no máximo 100 caracteres." }
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
        CreateProductCommand command, string propertyName, string expectedMessage)
    {
        // Arrange: command comes from InvalidCommands

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(propertyName).WithErrorMessage(expectedMessage);
    }
}