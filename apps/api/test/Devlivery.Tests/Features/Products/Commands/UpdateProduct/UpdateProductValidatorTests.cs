using Devlivery.Features.Products.Commands.UpdateProduct;

using FluentValidation.TestHelper;

using Shouldly;

namespace Devlivery.Tests.Features.Products.Commands.UpdateProduct;

[Trait("Category", "Unit Tests")]
public sealed class UpdateProductValidatorTests
{
    private static readonly UpdateProductCommand ValidCommand =
        new(Guid.NewGuid(), "Pizza Calabresa", "Molho, calabresa e cebola", 49.90m, "Pizzas", Available: true);

    private readonly UpdateProductCommandValidator _validator = new();

    public static TheoryData<UpdateProductCommand, string, string> InvalidCommands => new()
    {
        { ValidCommand with { Id = Guid.Empty }, "Id", "O campo 'Produto' é obrigatório." },
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
        UpdateProductCommand command, string propertyName, string expectedMessage)
    {
        // Arrange: command comes from InvalidCommands

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(propertyName).WithErrorMessage(expectedMessage);
    }
}