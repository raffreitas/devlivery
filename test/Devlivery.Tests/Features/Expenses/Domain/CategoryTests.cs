using Devlivery.Features.Expenses.Domain.Aggregates.Categories;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Domain;

[Collection("Expenses Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class CategoryTests(ExpensesUnitTestFixture fixture)
{
    [Fact]
    public void Constructor_Should_Create_Category_With_Correct_Properties()
    {
        // Arrange
        const string name = "Categoria Teste";
        var establishmentId = Guid.NewGuid();

        // Act
        var category = new Category(name, establishmentId);

        // Assert
        category.Name.ShouldBe(name);
        category.EstablishmentId.ShouldBe(establishmentId);
        category.IsActive.ShouldBeTrue();
        category.ParentCategoryId.ShouldBeNull();
        category.Subcategories.ShouldBeEmpty();
        category.CreatedAt.ShouldNotBe(default);
        category.UpdatedAt.ShouldNotBe(default);
    }

    [Fact]
    public void AddSubcategory_Should_Add_Subcategory_To_List()
    {
        // Arrange
        var parentCategory = fixture.CreateCategory();
        var subcategory = fixture.CreateCategory();

        // Act
        parentCategory.AddSubcategory(subcategory);

        // Assert
        parentCategory.Subcategories.ShouldContain(subcategory);
        subcategory.ParentCategoryId.ShouldBe(parentCategory.Id);
    }

    [Fact]
    public async Task AddSubcategory_Should_Update_UpdatedAt_Timestamp()
    {
        // Arrange
        var parentCategory = fixture.CreateCategory();
        var originalUpdatedAt = parentCategory.UpdatedAt;
        await Task.Delay(10);
        var subcategory = fixture.CreateCategory();

        // Act
        parentCategory.AddSubcategory(subcategory);

        // Assert
        parentCategory.UpdatedAt.ShouldBeGreaterThan(originalUpdatedAt);
    }

    [Fact]
    public void AddSubcategory_Should_Throw_When_Subcategory_Already_Added()
    {
        // Arrange
        var parentCategory = fixture.CreateCategory();
        var subcategory = fixture.CreateCategory();
        parentCategory.AddSubcategory(subcategory);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => parentCategory.AddSubcategory(subcategory));
    }

    [Fact]
    public void AddSubcategory_Should_Throw_When_Subcategory_Has_Parent()
    {
        // Arrange
        var parentCategory1 = fixture.CreateCategory();
        var parentCategory2 = fixture.CreateCategory();
        var subcategory = fixture.CreateCategory();
        parentCategory1.AddSubcategory(subcategory);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => parentCategory2.AddSubcategory(subcategory));
    }

    [Fact]
    public void Update_Should_Update_Name_When_Provided()
    {
        // Arrange
        const string originalName = "Nome Original";
        var category = fixture.CreateCategory(name: originalName);
        const string newName = "Nome Atualizado";

        // Act
        category.Update(name: newName);

        // Assert
        category.Name.ShouldBe(newName);
    }

    [Fact]
    public void Update_Should_Update_IsActive_When_Provided()
    {
        // Arrange
        var category = fixture.CreateCategory(isActive: true);

        // Act
        category.Update(isActive: false);

        // Assert
        category.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void Update_Should_Keep_Original_Values_When_Null()
    {
        // Arrange
        const string originalName = "Nome Original";
        var category = fixture.CreateCategory(name: originalName, isActive: true);

        // Act
        category.Update(name: null, isActive: null);

        // Assert
        category.Name.ShouldBe(originalName);
        category.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Update_Should_Update_UpdatedAt_Timestamp()
    {
        // Arrange
        var category = fixture.CreateCategory();
        var originalUpdatedAt = category.UpdatedAt;
        await Task.Delay(10);

        // Act
        category.Update(name: "Novo Nome");

        // Assert
        category.UpdatedAt.ShouldBeGreaterThan(originalUpdatedAt);
    }

    [Fact]
    public void Deactivate_Should_Set_IsActive_To_False()
    {
        // Arrange
        var category = fixture.CreateCategory(isActive: true);

        // Act
        category.Deactivate();

        // Assert
        category.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task Deactivate_Should_Update_UpdatedAt_Timestamp()
    {
        // Arrange
        var category = fixture.CreateCategory(isActive: true);
        var originalUpdatedAt = category.UpdatedAt;
        await Task.Delay(10);

        // Act
        category.Deactivate();

        // Assert
        category.UpdatedAt.ShouldBeGreaterThan(originalUpdatedAt);
    }

    [Fact]
    public void Activate_Should_Set_IsActive_To_True()
    {
        // Arrange
        var category = fixture.CreateCategory(isActive: false);

        // Act
        category.Activate();

        // Assert
        category.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Activate_Should_Update_UpdatedAt_Timestamp()
    {
        // Arrange
        var category = fixture.CreateCategory(isActive: false);
        var originalUpdatedAt = category.UpdatedAt;
        await Task.Delay(10);

        // Act
        category.Activate();

        // Assert
        category.UpdatedAt.ShouldBeGreaterThan(originalUpdatedAt);
    }
}

