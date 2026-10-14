using Devlivery.Features.Expenses.Commands.MarkExpenseAsPaid;

using FluentValidation.TestHelper;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.MarkExpenseAsPaid;

[Trait("Category", "Unit Tests")]
public sealed class MarkExpenseAsPaidValidatorTests
{
    private static readonly MarkExpenseAsPaidCommand ValidCommand = new(Guid.NewGuid(), new DateOnly(2026, 5, 8));

    private readonly MarkExpenseAsPaidCommandValidator _validator = new();

    public static TheoryData<MarkExpenseAsPaidCommand, string, string> InvalidCommands => new()
    {
        { ValidCommand with { ExpenseId = Guid.Empty }, "ExpenseId", "O campo 'Despesa' é obrigatório." },
        { ValidCommand with { PaymentDate = default }, "PaymentDate", "O campo 'Data de pagamento' é obrigatório." }
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
        MarkExpenseAsPaidCommand command, string propertyName, string expectedMessage)
    {
        // Arrange: command comes from InvalidCommands

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(propertyName).WithErrorMessage(expectedMessage);
    }
}