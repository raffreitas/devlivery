using Devlivery.Common.Errors;
using Devlivery.Features.Expenses.Commands.DeleteCategory;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.DeleteCategory;

[Trait("Category", "Unit Tests")]
[Collection("Expenses Unit Tests")]
public sealed class DeleteCategoryHandlerTests(ExpensesUnitTestFixture fixture)
{
    [Fact]
    public async Task Handle_WithCategoryNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var categoryRepository = fixture.CreateCategoryRepositoryMock();
        categoryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .ReturnsNull();

        var expenseRepository = fixture.CreateExpenseRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        var handler = new DeleteCategoryHandler(categoryRepository, expenseRepository, unitOfWork);

        var command = new DeleteCategoryCommand(Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e is NotFoundError);
        await categoryRepository.Received().GetByIdAsync(command.CategoryId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithActiveExpenses_ReturnsValidationError()
    {
        // Arrange
        var category = fixture.CreateCategory();
        var categoryRepository = fixture.CreateCategoryRepositoryMock();
        categoryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(category);

        var expenseRepository = fixture.CreateExpenseRepositoryMock();
        expenseRepository.ExistsWithCategoryAsync(category.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        var unitOfWork = fixture.CreateUnitOfWorkMock();

        var handler = new DeleteCategoryHandler(categoryRepository, expenseRepository, unitOfWork);

        var command = new DeleteCategoryCommand(category.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e is ValidationError);
        await expenseRepository.Received().ExistsWithCategoryAsync(category.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithParentCategory_DeactivatesCategoryAndSubcategories()
    {
        // Arrange
        var category = fixture.CreateCategory();
        var subcategory1 = fixture.CreateCategory();
        var subcategory2 = fixture.CreateCategory();
        category.AddSubcategory(subcategory1);
        category.AddSubcategory(subcategory2);

        var categoryRepository = fixture.CreateCategoryRepositoryMock();
        categoryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(category);

        var expenseRepository = fixture.CreateExpenseRepositoryMock();
        expenseRepository.ExistsWithCategoryAsync(category.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        var unitOfWork = fixture.CreateUnitOfWorkMock();

        var handler = new DeleteCategoryHandler(categoryRepository, expenseRepository, unitOfWork);

        var command = new DeleteCategoryCommand(category.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        category.IsActive.ShouldBeFalse();
        subcategory1.IsActive.ShouldBeFalse();
        subcategory2.IsActive.ShouldBeFalse();
        await categoryRepository.Received().UpdateAsync(category, Arg.Any<CancellationToken>());
        await unitOfWork.Received().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithSubcategory_DeactivatesOnlySubcategory()
    {
        // Arrange
        var parentCategory = fixture.CreateCategory();
        var subcategory = fixture.CreateCategory();
        parentCategory.AddSubcategory(subcategory);

        var categoryRepository = fixture.CreateCategoryRepositoryMock();
        categoryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(subcategory);

        var expenseRepository = fixture.CreateExpenseRepositoryMock();
        expenseRepository.ExistsWithCategoryAsync(subcategory.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        var unitOfWork = fixture.CreateUnitOfWorkMock();

        var handler = new DeleteCategoryHandler(categoryRepository, expenseRepository, unitOfWork);

        var command = new DeleteCategoryCommand(subcategory.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        subcategory.IsActive.ShouldBeFalse();
        parentCategory.IsActive.ShouldBeTrue(); // Parent should not be deactivated
        await categoryRepository.Received().UpdateAsync(subcategory, Arg.Any<CancellationToken>());
        await unitOfWork.Received().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidCategory_DeactivatesAndReturnsSuccess()
    {
        // Arrange
        var category = fixture.CreateCategory();
        var categoryRepository = fixture.CreateCategoryRepositoryMock();
        categoryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(category);

        var expenseRepository = fixture.CreateExpenseRepositoryMock();
        expenseRepository.ExistsWithCategoryAsync(category.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        var unitOfWork = fixture.CreateUnitOfWorkMock();

        var handler = new DeleteCategoryHandler(categoryRepository, expenseRepository, unitOfWork);

        var command = new DeleteCategoryCommand(category.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        category.IsActive.ShouldBeFalse();
        await categoryRepository.Received().UpdateAsync(category, Arg.Any<CancellationToken>());
        await unitOfWork.Received().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}