using Devlivery.Features.Expenses.Commands.DeleteExpense;

using FluentValidation.TestHelper;

namespace Devlivery.Tests.Features.Expenses.Commands.DeleteExpense;

[Collection("Expenses Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class DeleteExpenseValidatorTests
{
    private readonly DeleteExpenseCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_Command_Is_Valid()
    {
        // Arrange
        var command = new DeleteExpenseCommand(ExpenseId: Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Should_Fail_When_ExpenseId_Is_Empty()
    {
        // Arrange
        var command = new DeleteExpenseCommand(ExpenseId: Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ExpenseId);
    }
}