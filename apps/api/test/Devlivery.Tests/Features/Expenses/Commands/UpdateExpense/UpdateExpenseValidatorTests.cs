using Devlivery.Features.Expenses.Commands.UpdateExpense;

using FluentValidation.TestHelper;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.UpdateExpense;

[Trait("Category", "Unit Tests")]
public sealed class UpdateExpenseValidatorTests
{
    private static readonly UpdateExpenseCommand ValidCommand =
        new(Guid.NewGuid(), CategoryId: null, Amount: null, DueDate: null, Supplier: null, Description: null);

    private readonly UpdateExpenseCommandValidator _validator = new();

    public static TheoryData<UpdateExpenseCommand, string, string> InvalidCommands => new()
    {
        { ValidCommand with { ExpenseId = Guid.Empty }, "ExpenseId", "O campo 'Despesa' é obrigatório." },
        { ValidCommand with { Amount = 0m }, "Amount", "O campo 'Valor' deve ser maior que 0." },
        { ValidCommand with { PaymentDate = DateOnly.MinValue }, "PaymentDate", "O campo 'Data de pagamento' deve ser uma data válida." }
    };

    [Fact]
    public void GivenCommandWithOnlyExpenseId_WhenValidating_ThenHasNoErrors()
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
        UpdateExpenseCommand command, string propertyName, string expectedMessage)
    {
        // Arrange: command comes from InvalidCommands

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(propertyName).WithErrorMessage(expectedMessage);
    }
}