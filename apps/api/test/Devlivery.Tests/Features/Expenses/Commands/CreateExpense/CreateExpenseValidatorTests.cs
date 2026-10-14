using Devlivery.Features.Expenses.Commands.CreateExpense;

using FluentValidation.TestHelper;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.CreateExpense;

[Trait("Category", "Unit Tests")]
public sealed class CreateExpenseValidatorTests
{
    private static readonly CreateExpenseCommand ValidCommand =
        new(Guid.NewGuid(), 0.01m, new DateOnly(2026, 5, 10), Supplier: null, Description: null, PaymentDate: null);

    private readonly CreateExpenseCommandValidator _validator = new();

    public static TheoryData<CreateExpenseCommand, string, string> InvalidCommands => new()
    {
        { ValidCommand with { CategoryId = Guid.Empty }, "CategoryId", "O campo 'Categoria' é obrigatório." },
        { ValidCommand with { Amount = 0m }, "Amount", "O campo 'Valor' deve ser maior que 0." },
        { ValidCommand with { Amount = -10m }, "Amount", "O campo 'Valor' deve ser maior que 0." },
        { ValidCommand with { DueDate = default }, "DueDate", "O campo 'Data de vencimento' é obrigatório." }
    };

    [Fact]
    public void GivenCommandWithOnlyRequiredFields_WhenValidating_ThenHasNoErrors()
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
        CreateExpenseCommand command, string propertyName, string expectedMessage)
    {
        // Arrange: command comes from InvalidCommands

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(propertyName).WithErrorMessage(expectedMessage);
    }
}