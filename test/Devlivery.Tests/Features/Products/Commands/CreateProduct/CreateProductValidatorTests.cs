using Devlivery.Features.Products.Commands.CreateProduct;

using FluentValidation.TestHelper;

using Shouldly;

namespace Devlivery.Tests.Features.Products.Commands.CreateProduct;

[Collection("Products Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class CreateProductValidatorTests(ProductsUnitTestFixture fixture)
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public void Should_Pass_Validation_When_Command_Is_Valid()
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: fixture.Faker.Commerce.ProductName(),
            Description: fixture.Faker.Lorem.Sentence(),
            Price: fixture.Faker.Random.Decimal(1, 1000),
            Category: fixture.Faker.Commerce.Categories(1)[0],
            Available: true
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Fail_Validation_When_Name_Is_Empty()
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: string.Empty,
            Description: fixture.Faker.Lorem.Sentence(),
            Price: 10.99m,
            Category: "Categoria",
            Available: true
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("O campo 'Name' é obrigatório.");
    }

    [Fact]
    public void Should_Fail_Validation_When_Name_Exceeds_MaxLength()
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: new string('A', 201), // 201 caracteres
            Description: fixture.Faker.Lorem.Sentence(),
            Price: 10.99m,
            Category: "Categoria",
            Available: true
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("O campo 'Name' deve ter no máximo 200 caracteres.");
    }

    [Fact]
    public void Should_Fail_Validation_When_Description_Is_Empty()
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: fixture.Faker.Commerce.ProductName(),
            Description: string.Empty,
            Price: 10.99m,
            Category: "Categoria",
            Available: true
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("O campo 'Description' é obrigatório.");
    }

    [Fact]
    public void Should_Fail_Validation_When_Description_Exceeds_MaxLength()
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: fixture.Faker.Commerce.ProductName(),
            Description: new string('A', 1001), // 1001 caracteres
            Price: 10.99m,
            Category: "Categoria",
            Available: true
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("O campo 'Description' deve ter no máximo 1000 caracteres.");
    }

    [Fact]
    public void Should_Fail_Validation_When_Price_Is_Zero()
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: fixture.Faker.Commerce.ProductName(),
            Description: fixture.Faker.Lorem.Sentence(),
            Price: 0m,
            Category: "Categoria",
            Available: true
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("O campo 'Price' deve ser maior que 0.");
    }

    [Fact]
    public void Should_Fail_Validation_When_Price_Is_Negative()
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: fixture.Faker.Commerce.ProductName(),
            Description: fixture.Faker.Lorem.Sentence(),
            Price: -10.99m,
            Category: "Categoria",
            Available: true
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("O campo 'Price' deve ser maior que 0.");
    }

    [Fact]
    public void Should_Fail_Validation_When_Category_Is_Empty()
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: fixture.Faker.Commerce.ProductName(),
            Description: fixture.Faker.Lorem.Sentence(),
            Price: 10.99m,
            Category: string.Empty,
            Available: true
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Category)
            .WithErrorMessage("O campo 'Category' é obrigatório.");
    }

    [Fact]
    public void Should_Fail_Validation_When_Category_Exceeds_MaxLength()
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: fixture.Faker.Commerce.ProductName(),
            Description: fixture.Faker.Lorem.Sentence(),
            Price: 10.99m,
            Category: new string('A', 101), // 101 caracteres
            Available: true
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Category)
            .WithErrorMessage("O campo 'Category' deve ter no máximo 100 caracteres.");
    }
}