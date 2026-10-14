using Devlivery.Domain.Aggregates.Expenses;
using Devlivery.Domain.SeedWork;
using Devlivery.Tests.Common.Builders;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Domain;

[Trait("Category", "Unit Tests")]
public sealed class CategoryTests
{
    [Fact]
    public void GivenNameAndEstablishment_WhenCreatingCategory_ThenIsActiveTopLevelCategory()
    {
        // Arrange
        var establishmentId = Guid.NewGuid();

        // Act
        var category = new Category("Operacional", establishmentId);

        // Assert
        category.Name.ShouldBe("Operacional");
        category.EstablishmentId.ShouldBe(establishmentId);
        category.IsActive.ShouldBeTrue();
        category.ParentCategoryId.ShouldBeNull();
        category.Subcategories.ShouldBeEmpty();
    }

    [Fact]
    public void GivenTopLevelCategory_WhenAddingSubcategory_ThenLinksSubcategoryToParent()
    {
        // Arrange
        var parent = new CategoryBuilder().Build();
        var subcategory = new CategoryBuilder().Build();

        // Act
        parent.AddSubcategory(subcategory);

        // Assert
        parent.Subcategories.ShouldHaveSingleItem().ShouldBe(subcategory);
        subcategory.ParentCategoryId.ShouldBe(parent.Id);
    }

    [Fact]
    public void GivenSubcategoryAlreadyInParent_WhenAddingAgain_ThenThrowsDomainException()
    {
        // Arrange
        var parent = new CategoryBuilder().Build();
        var subcategory = new CategoryBuilder().WithParent(parent).Build();

        // Act
        var exception = Record.Exception(() => parent.AddSubcategory(subcategory));

        // Assert
        exception.ShouldBeOfType<DomainException>()
            .Message.ShouldBe("A subcategoria já está associada a esta categoria.");
        parent.Subcategories.ShouldHaveSingleItem();
    }

    [Fact]
    public void GivenSubcategoryOfAnotherParent_WhenAdding_ThenThrowsDomainException()
    {
        // Arrange
        var originalParent = new CategoryBuilder().Build();
        var subcategory = new CategoryBuilder().WithParent(originalParent).Build();
        var newParent = new CategoryBuilder().Build();

        // Act
        var exception = Record.Exception(() => newParent.AddSubcategory(subcategory));

        // Assert
        exception.ShouldBeOfType<DomainException>()
            .Message.ShouldBe("A subcategoria já está associada a outra categoria.");
        subcategory.ParentCategoryId.ShouldBe(originalParent.Id);
        newParent.Subcategories.ShouldBeEmpty();
    }

    [Fact]
    public void GivenCategory_WhenUpdatingNameAndStatus_ThenAppliesBoth()
    {
        // Arrange
        var category = new CategoryBuilder().WithName("Operacional").Build();
        var beforeUpdate = DateTime.UtcNow;

        // Act
        category.Update("Operação", isActive: false);

        // Assert
        category.Name.ShouldBe("Operação");
        category.IsActive.ShouldBeFalse();
        category.UpdatedAt.ShouldBeGreaterThanOrEqualTo(beforeUpdate);
    }

    [Fact]
    public void GivenCategory_WhenUpdatingWithNullFields_ThenKeepsOriginalValues()
    {
        // Arrange
        var category = new CategoryBuilder().WithName("Operacional").Build();

        // Act
        category.Update();

        // Assert
        category.Name.ShouldBe("Operacional");
        category.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void GivenActiveCategory_WhenDeactivating_ThenIsInactive()
    {
        // Arrange
        var category = new CategoryBuilder().Build();
        var beforeUpdate = DateTime.UtcNow;

        // Act
        category.Deactivate();

        // Assert
        category.IsActive.ShouldBeFalse();
        category.UpdatedAt.ShouldBeGreaterThanOrEqualTo(beforeUpdate);
    }
}