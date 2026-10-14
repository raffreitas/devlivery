using Devlivery.Common.Errors;
using Devlivery.Domain.Aggregates.Expenses.Abstractions;
using Devlivery.Features.Expenses.Commands.DeleteCategory;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Tests.Common.Builders;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.DeleteCategory;

[Trait("Category", "Unit Tests")]
public sealed class DeleteCategoryHandlerTests
{
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly IExpenseRepository _expenseRepository = Substitute.For<IExpenseRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteCategoryHandler _handler;

    public DeleteCategoryHandlerTests()
    {
        _handler = new DeleteCategoryHandler(_categoryRepository, _expenseRepository, _unitOfWork);
    }

    [Fact]
    public async Task GivenNonexistentCategory_WhenDeleting_ThenReturnsNotFoundError()
    {
        // Arrange
        _categoryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();

        // Act
        var result = await _handler.HandleAsync(new DeleteCategoryCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.HasError<NotFoundError>().ShouldBeTrue();
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(false, "Não é possível excluir a categoria pois existem despesas ativas associadas a ela.")]
    [InlineData(true, "Não é possível excluir a subcategoria pois existem despesas ativas associadas a ela.")]
    public async Task GivenCategoryWithExpenses_WhenDeleting_ThenReturnsValidationErrorAndKeepsItActive(
        bool isSubcategory, string expectedMessage)
    {
        // Arrange
        var builder = new CategoryBuilder();
        var category = (isSubcategory ? builder.WithParent(new CategoryBuilder().Build()) : builder).Build();
        _categoryRepository.GetByIdAsync(category.Id, Arg.Any<CancellationToken>()).Returns(category);
        _expenseRepository.ExistsWithCategoryAsync(category.Id, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _handler.HandleAsync(new DeleteCategoryCommand(category.Id), CancellationToken.None);

        // Assert
        result.Errors.ShouldHaveSingleItem().ShouldBeOfType<ValidationError>().Message.ShouldBe(expectedMessage);
        category.IsActive.ShouldBeTrue();
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenTopLevelCategoryWithoutExpenses_WhenDeleting_ThenDeactivatesItAndItsSubcategories()
    {
        // Arrange
        var parent = new CategoryBuilder().Build();
        var firstSubcategory = new CategoryBuilder().WithParent(parent).Build();
        var secondSubcategory = new CategoryBuilder().WithParent(parent).Build();
        _categoryRepository.GetByIdAsync(parent.Id, Arg.Any<CancellationToken>()).Returns(parent);

        // Act
        var result = await _handler.HandleAsync(new DeleteCategoryCommand(parent.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        parent.IsActive.ShouldBeFalse();
        firstSubcategory.IsActive.ShouldBeFalse();
        secondSubcategory.IsActive.ShouldBeFalse();
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenSubcategoryWithoutExpenses_WhenDeleting_ThenDeactivatesOnlyTheSubcategory()
    {
        // Arrange
        var parent = new CategoryBuilder().Build();
        var subcategory = new CategoryBuilder().WithParent(parent).Build();
        var sibling = new CategoryBuilder().WithParent(parent).Build();
        _categoryRepository.GetByIdAsync(subcategory.Id, Arg.Any<CancellationToken>()).Returns(subcategory);

        // Act
        var result = await _handler.HandleAsync(new DeleteCategoryCommand(subcategory.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        subcategory.IsActive.ShouldBeFalse();
        parent.IsActive.ShouldBeTrue();
        sibling.IsActive.ShouldBeTrue();
    }
}