using Devlivery.Features.Expenses.Commands.CreateExpense;

using FluentValidation.TestHelper;

namespace Devlivery.Tests.Features.Expenses.Commands.CreateExpense;

[Collection("Expenses Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class CreateExpenseValidatorTests
{
    private readonly CreateExpenseCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_Command_Is_Valid()
    {
        // Arrange
        var command = new CreateExpenseCommand(
            CategoryId: Guid.NewGuid(),
            Amount: 100.00m,
            DueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Supplier: "Fornecedor Teste",
            Description: "Descrição Teste",
            PaymentDate: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Should_Fail_When_CategoryId_Is_Empty()
    {
        // Arrange
        var command = new CreateExpenseCommand(
            CategoryId: Guid.Empty,
            Amount: 100.00m,
            DueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Supplier: null,
            Description: null,
            PaymentDate: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }

    [Fact]
    public void Validate_Should_Fail_When_Amount_Is_Zero()
    {
        // Arrange
        var command = new CreateExpenseCommand(
            CategoryId: Guid.NewGuid(),
            Amount: 0,
            DueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Supplier: null,
            Description: null,
            PaymentDate: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Validate_Should_Fail_When_Amount_Is_Negative()
    {
        // Arrange
        var command = new CreateExpenseCommand(
            CategoryId: Guid.NewGuid(),
            Amount: -10.00m,
            DueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Supplier: null,
            Description: null,
            PaymentDate: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Validate_Should_Pass_When_Supplier_Is_Null()
    {
        // Arrange
        var command = new CreateExpenseCommand(
            CategoryId: Guid.NewGuid(),
            Amount: 100.00m,
            DueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Supplier: null,
            Description: null,
            PaymentDate: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Supplier);
    }

    [Fact]
    public void Validate_Should_Pass_When_Description_Is_Null()
    {
        // Arrange
        var command = new CreateExpenseCommand(
            CategoryId: Guid.NewGuid(),
            Amount: 100.00m,
            DueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Supplier: null,
            Description: null,
            PaymentDate: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_Should_Pass_When_PaymentDate_Is_Null()
    {
        // Arrange
        var command = new CreateExpenseCommand(
            CategoryId: Guid.NewGuid(),
            Amount: 100.00m,
            DueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Supplier: null,
            Description: null,
            PaymentDate: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PaymentDate);
    }
}

