using Devlivery.Features.Expenses.Commands.UpdateExpense;

using FluentValidation.TestHelper;

namespace Devlivery.Tests.Features.Expenses.Commands.UpdateExpense;

[Collection("Expenses Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class UpdateExpenseValidatorTests
{
    private readonly UpdateExpenseCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_Command_Is_Valid()
    {
        // Arrange
        var command = new UpdateExpenseCommand(
            ExpenseId: Guid.NewGuid(),
            CategoryId: Guid.NewGuid(),
            Amount: 200.00m,
            DueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
            Supplier: "Fornecedor Atualizado",
            Description: "Descrição Atualizada"
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
        var command = new UpdateExpenseCommand(
            ExpenseId: Guid.Empty,
            CategoryId: null,
            Amount: null,
            DueDate: null,
            Supplier: null,
            Description: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ExpenseId);
    }

    [Fact]
    public void Validate_Should_Fail_When_Amount_Is_Zero()
    {
        // Arrange
        var command = new UpdateExpenseCommand(
            ExpenseId: Guid.NewGuid(),
            CategoryId: null,
            Amount: 0,
            DueDate: null,
            Supplier: null,
            Description: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount!.Value);
    }

    [Fact]
    public void Validate_Should_Fail_When_Amount_Is_Negative()
    {
        // Arrange
        var command = new UpdateExpenseCommand(
            ExpenseId: Guid.NewGuid(),
            CategoryId: null,
            Amount: -10.00m,
            DueDate: null,
            Supplier: null,
            Description: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount!.Value);
    }

    [Fact]
    public void Validate_Should_Pass_When_All_Optional_Fields_Are_Null()
    {
        // Arrange
        var command = new UpdateExpenseCommand(
            ExpenseId: Guid.NewGuid(),
            CategoryId: null,
            Amount: null,
            DueDate: null,
            Supplier: null,
            Description: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}

