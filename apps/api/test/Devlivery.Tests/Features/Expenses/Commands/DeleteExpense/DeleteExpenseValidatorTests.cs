using Devlivery.Features.Expenses.Commands.DeleteExpense;

using FluentValidation.TestHelper;

namespace Devlivery.Tests.Features.Expenses.Commands.DeleteExpense;

[Trait("Category", "Unit Tests")]
public sealed class DeleteExpenseValidatorTests
{
    private readonly DeleteExpenseCommandValidator _validator = new();

    [Fact]
    public void GivenEmptyExpenseId_WhenValidating_ThenReturnsRequiredExpenseError()
    {
        // Arrange
        var command = new DeleteExpenseCommand(Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ExpenseId).WithErrorMessage("O campo 'Despesa' é obrigatório.");
    }
}