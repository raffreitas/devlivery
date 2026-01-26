using Devlivery.Common.Errors;
using Devlivery.Domain.Aggregates.Expenses;
using Devlivery.Features.Expenses.Commands.UpdateCategory;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.UpdateCategory;

[Trait("Category", "Unit Tests")]
[Collection("Expenses Unit Tests")]
public sealed class UpdateCategoryHandlerTests(ExpensesUnitTestFixture fixture)
{
    [Fact]
    public async Task Handle_Should_Return_NotFoundError_When_Category_Does_Not_Exist()
    {
        // Arrange
        var categoryRepository = fixture.CreateCategoryRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        var handler = new UpdateCategoryHandler(categoryRepository, unitOfWork);

        var command = new UpdateCategoryCommand(Guid.NewGuid(), "new-name", true);

        categoryRepository.GetByIdAsync(command.CategoryId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e is NotFoundError);
        await categoryRepository.DidNotReceive().UpdateAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Return_ValidationError_When_Name_Already_Exists()
    {
        // Arrange
        var categoryRepository = fixture.CreateCategoryRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        var existingCategory = fixture.CreateCategory(name: "old-name");

        categoryRepository.GetByIdAsync(existingCategory.Id, Arg.Any<CancellationToken>())
            .Returns(existingCategory);

        // Simulate that a different category with the new name already exists
        categoryRepository.ExistsWithName("new-name", existingCategory.ParentCategoryId, Arg.Any<CancellationToken>())
            .Returns(true);

        var handler = new UpdateCategoryHandler(categoryRepository, unitOfWork);

        var command = new UpdateCategoryCommand(existingCategory.Id, "new-name", null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e is ValidationError);
        await categoryRepository.DidNotReceive().UpdateAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Update_Category_And_Save_When_Successful()
    {
        // Arrange
        var categoryRepository = fixture.CreateCategoryRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        var existingCategory = fixture.CreateCategory(name: "same-name");

        categoryRepository.GetByIdAsync(existingCategory.Id, Arg.Any<CancellationToken>())
            .Returns(existingCategory);

        // If name is same as current, ExistsWithName should not cause a conflict (we'll return false)
        categoryRepository.ExistsWithName(Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var handler = new UpdateCategoryHandler(categoryRepository, unitOfWork);

        var command = new UpdateCategoryCommand(existingCategory.Id, "same-name", false);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        await categoryRepository.Received(1)
            .UpdateAsync(
                Arg.Is<Category>(c => c.Id == existingCategory.Id && c.Name == "same-name" && !c.IsActive),
                Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}