using Devlivery.Features.Expenses.Commands.MarkExpenseAsPaid;

using FluentValidation.TestHelper;

namespace Devlivery.Tests.Features.Expenses.Commands.MarkExpenseAsPaid;

[Collection("Expenses Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class MarkExpenseAsPaidValidatorTests
{
    private readonly MarkExpenseAsPaidCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_Command_Is_Valid()
    {
        // Arrange
        var command = new MarkExpenseAsPaidCommand(
            ExpenseId: Guid.NewGuid(),
            PaymentDate: DateOnly.FromDateTime(DateTime.UtcNow)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Should_Fail_When_ExpenseId_Is_Empty()
    {
        // Arrange
        var command = new MarkExpenseAsPaidCommand(
            ExpenseId: Guid.Empty,
            PaymentDate: DateOnly.FromDateTime(DateTime.UtcNow)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ExpenseId);
    }

    [Fact]
    public void Validate_Should_Fail_When_PaymentDate_Is_Default()
    {
        // Arrange
        var command = new MarkExpenseAsPaidCommand(
            ExpenseId: Guid.NewGuid(),
            PaymentDate: default
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PaymentDate);
    }
}

